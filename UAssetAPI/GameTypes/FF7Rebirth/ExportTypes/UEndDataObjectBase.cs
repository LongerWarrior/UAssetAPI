using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using UAssetAPI.ExportTypes;
using UAssetAPI.GameTypes.FF7Rebirth.PropertyTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using static UAssetAPI.UAPUtils;

namespace UAssetAPI.GameTypes.FF7Rebirth.ExportTypes;

public class UMemoryMappedAsset : NormalExport
{
    [JsonIgnore] protected FMemoryMappedImageResult MemoryMappedImage;
    [JsonIgnore] public FMemoryMappedImageArchive FrozenArchive = new FMemoryMappedImageArchive(null, new MemoryStream(), 0);

    public override void Read(AssetBinaryReader reader, int nextStarting = 0)
    {
        base.Read(reader, nextStarting);

        MemoryMappedImage = new FMemoryMappedImageResult();
        MemoryMappedImage.LoadFromArchive(reader);
        FrozenArchive = new FMemoryMappedImageArchive(reader.Asset, new MemoryStream(MemoryMappedImage.FrozenObject), MemoryMappedImage.FrozenOffset)
        {
            Names = MemoryMappedImage.GetNames()            
        };
    }

    public override void Write(AssetBinaryWriter writer)
    {
        base.Write(writer);
    }
}

public struct FKey : IFF7FrozenProperty
{
    public FName Name;
    public int Index;
    public int NextIndex;
    public int Priority;

    public FKey(FName name, int index, int nextIndex, int priority)
    {
        Name = name;
        Index = index;
        NextIndex = nextIndex;
        Priority = priority;
    }
    
    public FKey(FMemoryMappedImageArchive Ar)
    {
        Name = Ar.ReadFName();
        Index = Ar.ReadInt32();
        NextIndex = Ar.ReadInt32();
        Priority = Ar.ReadInt32();
    }

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.WriteFName(Name);
        Ar.Write(Index);
        Ar.Write(NextIndex);
        Ar.Write(Priority);
    }
}

public struct FF7Property(FMemoryMappedImageArchive Ar) : IFF7FrozenProperty
{
    public FName Name = Ar.ReadFName();
    public FF7PropertyType UnderlyingType = (FF7PropertyType)Ar.ReadInt32();
    
    public readonly bool isArray => Name.Value.Value.EndsWith("_Array");

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.WriteFName(Name);
        Ar.Write((int)UnderlyingType);
    }
}

public class UEndDataObjectBase : UMemoryMappedAsset
{
    public FKey[] Keys = [];
    public int[] Indexes = [];
    public FF7Property[] StructDefinition = [];

    public override void Read(AssetBinaryReader reader, int nextStarting = 0)
    {
        base.Read(reader, nextStarting);

        Keys = FrozenArchive.ReadTSparseArray(() => new FKey(FrozenArchive), 20).ToArray();
        Indexes = FrozenArchive.ReadArray<int>();
        StructDefinition = FrozenArchive.ReadArray(() => new FF7Property(FrozenArchive));
        var align = PropUtils.GetPropAlign(StructDefinition[0].UnderlyingType);
        FrozenArchive.Align(align);
        var values = FrozenArchive.ReadArray(() => FrozenArchive.DeserializeProperties(StructDefinition),align);
        Data = new List<PropertyData>(Keys.Length);
        for (var i = 0; i < Keys.Length; i++)
        {
            var structProp = values[i];
            structProp.Name = Keys[i].Name;
            Data.Add(structProp);
        }
    }

    public int[] GenerateIndex(List<FKey> keys)
    {
        var sorted = keys.OrderBy(x => x.Priority).ThenByDescending(x => x.NextIndex);
        var indices = new List<int>();
        var current = 0;
        foreach (var key in sorted)
        {
            var priority = key.Priority;
            if (priority > current)
            {
                indices.AddRange(Enumerable.Repeat(-1, priority - current));
            }
            else if(priority < current)
            {
                continue;
            }
            
            indices.Add(key.Index);
            current = priority + 1;
        }

        var currcount = (uint)indices.Count;
        var count = currcount == 1 ? currcount : Math.Max(16, BitOperations.RoundUpToPowerOf2(currcount));
        indices.AddRange(Enumerable.Repeat(-1, (int)(count - currcount)));
        return indices.ToArray();
    }

    public override void Write(AssetBinaryWriter writer)
    {
        var start = writer.BaseStream.Position;
        writer.Write(new FName(writer.Asset, "None"));
        writer.Write(0);

        var Ar = new FMemoryMappedImageWriter(new MemoryStream((int)FrozenArchive.BaseStream.Length));
        Ar.Names = new Dictionary<int, List<int>>[writer.Asset.NameCount];
        Ar.StructDefinition = StructDefinition;

        var keysDictionary = new Dictionary<FName, FKey>(Keys.Length);
        var maxpriority = 0;
        for (var i = 0; i < Keys.Length; i++)
        {
            keysDictionary[Keys[i].Name] = Keys[i];
            if (Keys[i].Priority > maxpriority) maxpriority = Keys[i].Priority;
        }
        
        var newKeys = new List<FKey>(Data.Count);
        for (var i = 0; i < Data.Count; i++)
        {
            var name = Data[i].Name;
            newKeys.Add(keysDictionary.TryGetValue(name, out var key) ? key : new FKey(name, i, -1, ++maxpriority));
            // adding new entries like this might double indices array, cause they usually round max indices count to the closest power of 2
        }

        // writing sparsearray manually
        var pointerOffset = Ar.WriteDummyPointer(newKeys.Count);
        Ar.AddQueue(new FF7OffsetProperty(pointerOffset));
        for (var i = 0; i < newKeys.Count; i++)
        {
            Ar.AddQueue(newKeys[i]);
        }

        // writing BitArray
        pointerOffset = Ar.WriteDummyPointer(newKeys.Count);
        var bitArray = new BitArray(newKeys.Count, true);
        int[] intArray = new int[(newKeys.Count + 31) / 32]; // Each int holds 32 bits
        bitArray.CopyTo(intArray, 0);
        Ar.AddQueue(new FF7ArrayView(MemoryMarshal.AsBytes<int>(intArray).ToArray(), pointerOffset));

        //freeindex and numfreeindex
        Ar.Write(-1);
        Ar.Write(0);

        Indexes = GenerateIndex(newKeys);
        // max is zero to preserve binary equality
        pointerOffset = Ar.WriteDummyPointer(Indexes.Length, 0);
        Ar.AddQueue(new FF7ArrayView(MemoryMarshal.AsBytes<int>(Indexes).ToArray(), pointerOffset));

        // writing struct definition
        pointerOffset = Ar.WriteDummyPointer(StructDefinition.Length);
        Ar.AddQueue(new FF7OffsetProperty(pointerOffset));
        foreach (var property in StructDefinition)
        {
            Ar.AddQueue(property);
        }

        // writing values
        pointerOffset = Ar.WriteDummyPointer(newKeys.Count);
        var align = PropUtils.GetPropAlign(Ar.StructDefinition[0].UnderlyingType);
        if (Ar.StructDefinition[0].isArray) align = 8;
        Ar.AddQueue(new FF7RealignProperty(align));
        Ar.AddQueue(new FF7OffsetProperty(pointerOffset));
        foreach (var key in Data)
        {
            Ar.AddQueue(new FF7RealignProperty(align));
            Ar.AddQueue(key as IFF7FrozenProperty);
        }

        Ar.Write();

        writer.Write((int)Ar.BaseStream.Length);
        writer.Write(65536);
        writer.Write((ushort)16);
        var current = writer.BaseStream.Position - start + 2;
        var padding = AlignPadding(current, 16) - current;
        writer.Write((ushort)padding);
        writer.Write(new byte[padding]);
        Ar.BaseStream.Seek(0, SeekOrigin.Begin);
        Ar.BaseStream.CopyTo(writer.BaseStream);

        writer.Write(0);
        writer.Write(0);

        var saved = writer.BaseStream.Position;
        writer.Write(0);

        var count = 0;

        for (var index = 0; index < Ar.Names.Length; index++)
        {
            if (Ar.Names[index] is null) continue;
            var dict = Ar.Names[index];
            var sorted = dict.OrderBy(x => x.Key);

            foreach (var (key, value) in sorted)
            {
                count++;
                writer.Write(index);
                writer.Write(key);
                writer.Write(value.Count);
                for (var i = 0; i < value.Count; i++)
                {
                    writer.Write(value[i]);
                }
            }
        }
        writer.BaseStream.Position = saved;
        writer.Write(count);
        writer.BaseStream.Position = writer.BaseStream.Length;
    }
}
