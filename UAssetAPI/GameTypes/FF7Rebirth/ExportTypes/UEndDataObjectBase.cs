using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

public struct FKey(FMemoryMappedImageArchive Ar) : IFF7FrozenProperty
{
    public FName Name = Ar.ReadFName();
    public int Index = Ar.ReadInt32();
    public int NextIndex = Ar.ReadInt32();
    public int Priority = Ar.ReadInt32();

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

    public override void Write(AssetBinaryWriter writer)
    {
        var start = writer.BaseStream.Position;
        writer.Write(new FName(writer.Asset, "None"));
        writer.Write(0);

        var Ar = new FMemoryMappedImageWriter(new MemoryStream((int)FrozenArchive.BaseStream.Length));
        Ar.Names = new Dictionary<int, List<int>>[writer.Asset.NameCount];
        Ar.StructDefinition = StructDefinition;

        // writing sparsearray manually
        var pointerOffset = Ar.WriteDummyPointer(Keys.Length);
        Ar.AddQueue(new FF7OffsetProperty(pointerOffset));
        for (var i = 0; i < Keys.Length; i++)
        {
            Ar.AddQueue(Keys[i]);
        }

        // writing BitArray
        pointerOffset = Ar.WriteDummyPointer(Keys.Length);
        var bitArray = new BitArray(Keys.Length, true);
        int[] intArray = new int[(Keys.Length + 31) / 32]; // Each int holds 32 bits
        bitArray.CopyTo(intArray, 0);
        Ar.AddQueue(new FF7ArrayView(MemoryMarshal.AsBytes<int>(intArray).ToArray(), pointerOffset));

        //freeindex and numfreeindex
        Ar.Write(-1);
        Ar.Write(0);

        // writing Indexes, later we can generate it from Keys, max is zero to preserve binary equality
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
        pointerOffset = Ar.WriteDummyPointer(Keys.Length);
        var align = PropUtils.GetPropAlign(StructDefinition[0].UnderlyingType);
        if (Data[0] is FF7StructProperty strukt && strukt.Value[0] is FF7ArrayProperty) align = 8;
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

        //writer.Write(Ar.Names.Count);
        //// sort by key
        //var names = Ar.Names.OrderBy(x => x.Key.Index).ThenBy(x => x.Key.Number);
        //foreach (var name in names)
        //{
        //    writer.Write(name.Key);
        //    writer.Write(name.Value.Count);
        //    for (var i = 0; i < name.Value.Count; i++)
        //    {
        //        writer.Write(name.Value[i]);
        //    }
        //}
    }
}
