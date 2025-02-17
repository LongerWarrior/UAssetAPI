using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UAssetAPI.GameTypes.FF7Rebirth.ExportTypes;
using UAssetAPI.GameTypes.FF7Rebirth.PropertyTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using static UAssetAPI.UAPUtils;

namespace UAssetAPI.GameTypes.FF7Rebirth;

public readonly struct FFrozenMemoryImagePtr(FMemoryMappedImageArchive Ar)
{
    public readonly ulong _packed = Ar.ReadUInt64();
    public readonly long OffsetFromThis => (long)_packed >> 1;
    public readonly bool IsFrozen => (_packed & 1) != 0;
}

public class FMemoryMappedImageArchive : UnrealBinaryReader
{
    public Dictionary<int, FName> Names;
    public readonly UAsset Asset;
    public readonly long FrozenOffset;
    public long Offset => FrozenOffset + BaseStream.Position; 
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public FMemoryMappedImageArchive(UAsset asset, Stream stream, long offset) : base(stream)
    {
        Asset = asset;
        FrozenOffset = offset;
    }

    public void Align(int align)
    {
        BaseStream.Position = AlignPadding(BaseStream.Position, align);
    }

    public FName ReadFName()
    {
        if (Names != null && Names.TryGetValue((int)Position, out FName name))
        {
            Position += 8;
            return name;
        }
        Position += 8;
        return default;
    }

    public override bool ReadBoolean()
    {
        return ReadByte() switch
        {
            1 => true,
            0 => false,
            _ => throw new FormatException("Invalid boolean value"),
        };
    }

    public virtual FF7String ReadFString()
    {
        var initialPos = Position;
        var dataPtr = new FFrozenMemoryImagePtr(this);
        var arrayNum = ReadInt32();
        var arrayMax = ReadInt32();
        if (arrayNum != arrayMax)
        {
            throw new FormatException($"Num ({arrayNum}) != Max ({arrayMax})");
        }
        if (arrayNum <= 1)
        {
            return null;
        }

        var continuePos = Position;
        Position = initialPos + dataPtr.OffsetFromThis;
        var ucs2Bytes = ReadBytes(arrayNum * 2);
        Position = continuePos;
        if (ucs2Bytes[^1] != 0 || ucs2Bytes[^2] != 0)
        {
            throw new FormatException("Serialized FString is not null terminated");
        }

        return new FF7String(Encoding.Unicode.GetString(ucs2Bytes, 0, ucs2Bytes.Length - 2), Encoding.Unicode);
    }

    public FF7StructProperty DeserializeProperties(FF7Property[] structProperties)
    {
        var properties = new List<PropertyData>(structProperties.Length);
        foreach (var prop in structProperties)
        {
            var tag = prop.isArray ? new FF7ArrayProperty(this, prop) : ReadPropertyData(prop);
            properties.Add(tag);
        }

        return new FF7StructProperty(properties);
    }
    
    public PropertyData ReadPropertyData(FF7Property property)
    {
        var offset = Offset;
        PropertyData prop = property.UnderlyingType switch
        {
            FF7PropertyType.BoolProperty => new FF7BoolProperty(this),
            FF7PropertyType.ByteProperty => new FF7ByteProperty(this),
            FF7PropertyType.Int8Property => new FF7Int8Property(this),
            FF7PropertyType.UInt16Property => new FF7UInt16Property(this),
            FF7PropertyType.Int16Property => new FF7Int16Property(this),
            FF7PropertyType.UIntProperty => new FF7UIntProperty(this),
            FF7PropertyType.IntProperty => new FF7IntProperty(this),
            FF7PropertyType.Int64Property => new FF7Int64Property(this),
            FF7PropertyType.FloatProperty => new FF7FloatProperty(this),
            FF7PropertyType.StrProperty => new FF7StrProperty(this),
            FF7PropertyType.NameProperty => new FF7NameProperty(this),
            _ => throw new FormatException($"Unknown property type {property.UnderlyingType}")
        };
        prop.Name = property.Name;
        prop.Offset = offset;
        return prop;
    }

    public virtual T[] ReadArray<T>(int length)
    {
        var size = Unsafe.SizeOf<T>();
        var readLength = size * length;

        var buffer = ReadBytes(readLength);
        var result = new T[length];
        if (length > 0) Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref result[0]), ref buffer[0], (uint)(readLength));
        return result;
    }

    public T[] ReadArray<T>()
    {
        var initialPos = Position;
        var dataPtr = new FFrozenMemoryImagePtr(this);
        var arrayNum = ReadInt32();
        var arrayMax = ReadInt32();
        if (arrayNum == 0)
        {
            return [];
        }

        var continuePos = Position;
        Position = initialPos + dataPtr.OffsetFromThis;
        var data = ReadArray<T>(arrayNum);
        Position = continuePos;
        return data;
    }

    public override T[] ReadArray<T>(Func<T> getter)
    {
        var initialPos = Position;
        var dataPtr = new FFrozenMemoryImagePtr(this);
        var arrayNum = ReadInt32();
        var arrayMax = ReadInt32();

        if (arrayNum == 0)
        {
            return [];
        }

        var continuePos = Position;
        Position = initialPos + dataPtr.OffsetFromThis;
        var data = new T[arrayNum];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = getter();
        }
        Position = continuePos;
        return data;
    }

    public T[] ReadArray<T>(Func<T> getter, int align)
    {
        var initialPos = Position;
        var dataPtr = new FFrozenMemoryImagePtr(this);
        var arrayNum = ReadInt32();
        var arrayMax = ReadInt32();
        if (arrayNum != arrayMax)
        {
            throw new FormatException($"Num ({arrayNum}) != Max ({arrayMax})");
        }
        if (arrayNum == 0)
        {
            return [];
        }

        var continuePos = Position;
        Position = initialPos + dataPtr.OffsetFromThis;
        var data = new T[arrayNum];
        for (int i = 0; i < data.Length; i++)
        {
            Align(align);
            data[i] = getter();
        }
        Position = continuePos;
        return data;
    }

    public new IEnumerable<ElementType> ReadTSparseArray<ElementType>(Func<ElementType> elementGetter, int elementStructSize)
    {
        var initialPos = Position;
        var dataPtr = new FFrozenMemoryImagePtr(this);
        var dataNum = ReadInt32();
        var dataMax = ReadInt32();
        var allocationFlags = ReadTBitArray();
        var FirstFreeIndex = ReadInt32();
        var NumFreeIndices = ReadInt32();

        if (dataNum == 0)
        {
            return [];
        }

        var continuePos = Position;
        Position = initialPos + dataPtr.OffsetFromThis;
        var data = new List<ElementType>(dataNum);
        for (var i = 0; i < dataNum; ++i)
        {
            var start = Position;
            if (allocationFlags[i])
            {
                data.Add(elementGetter());
            }
            Position = start + elementStructSize;
        }

        Position = continuePos;
        return data;
    }

    public BitArray ReadTBitArray()
    {
        var initialPos = Position;
        var dataPtr = new FFrozenMemoryImagePtr(this);
        var numBits = ReadInt32();
        var maxBits = ReadInt32();
        if (numBits == 0)
        {
            return new BitArray(0);
        }

        var continuePos = Position;
        Position = initialPos + dataPtr.OffsetFromThis;
        var data = ReadArray(DivideAndRoundUp(numBits, 32), ReadInt32);
        Position = continuePos;
        return new BitArray(data) { Length = numBits };
    }
}

public class FMemoryMappedImageResult
{
    public byte[] FrozenObject = [];
    public int BufferSize;
    public ushort MaxAlign;
    public long FrozenOffset;
    public FMemoryImageName[] Names = [];

    public void LoadFromArchive(AssetBinaryReader Ar)
    {
        var frozenSize = Ar.ReadInt32();
        BufferSize = Ar.ReadInt32();
        MaxAlign = Ar.ReadUInt16();
        var padding = Ar.ReadUInt16();
        Ar.BaseStream.Position += padding; //zeroed
        FrozenOffset = Ar.BaseStream.Position;
        FrozenObject = Ar.ReadBytes(frozenSize);
        Ar.BaseStream.Position += 8; // Skipping VTables and ScriptNames, anyways they are zeroed
        Names = Ar.ReadArray(() => new FMemoryImageName(Ar));
    }

    internal Dictionary<int, FName> GetNames()
    {
        var names = new Dictionary<int, FName>();

        foreach (var name in Names)
        {
            foreach (var patch in name.Patches)
            {
                names[patch] = name.Name;
            }
        }

        return names;
    }
}

public struct FMemoryImageName(AssetBinaryReader Ar)
{
    public FName Name = Ar.ReadFName();
    public int[] Patches = Ar.ReadArray(Ar.ReadInt32);
}

public class FMemoryMappedImageWriter(Stream stream, bool writeFName = false) : UnrealBinaryWriter(stream)
{
    public FF7Property[] StructDefinition;
    private Queue<IFF7FrozenProperty> WritingQueue = new();
    private Queue<IFF7FrozenProperty> IndirectDataQueue = new();
    private Queue<IFF7AltFrozenProperty> AltWritingQueue = new();
    private Dictionary<long, long> Pointers = [];
    public Dictionary<ulong, long> Arrays = [];
    public Dictionary<int,List<int>>[] Names = [];
    public bool bWriteFName = writeFName;

    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }
    
    public void AddPointer(long offset, long position)
    {
        Pointers[offset] = position;
    }

    public void AddName(FName name, int offset)
    {
        if (Names[name.Index] is { } val)
        {
            if (val.TryGetValue(name.Number, out var list))
            {
                list.Add(offset);
            }
            else
            {
                val[name.Number] = [offset];
            }
        }
        else
        {
            Names[name.Index] = new Dictionary<int, List<int>> { { name.Number, [offset] } };
        }
    }

    public void Write()
    {
        ProcessWritingQueue();
        if (!bWriteFName) AlignWriter(8);
        ProcessIndirectDataQueue();
        
        if (bWriteFName) return;
        AlignWriter(16);

        // writing pointers into corresponding offsets
        foreach (var (offset, position) in Pointers)
        {
            var res = ((position - offset) << 1) + 1;
            Position = offset;
            Write(res);
        }
    }

    public void AlignWriter(int align)
    {
        var len = AlignPadding(Position, align) - Position;
        BaseStream.SetLength(BaseStream.Length + len);
        Position += len;
    }

    public void WriteFName(FName name)
    {   
        AlignWriter(4);

        if (name is null)
        {
            if (bWriteFName)
            {
                Write((long)-1);
                return;
            }
            Write((long)0);
            return;
        }

        if (bWriteFName)
        {
            Write(name.Index);
            Write(name.Number);
            return;
        }

        AddName(name, (int)Position);

        Write((ulong)0);
    }

    public long WriteDummyPointer()
    {
        AlignWriter(8);
        var pos = Position;
        Write((ulong)0);
        return pos;
    }

    public long WriteDummyPointer(int length)
    {
        AlignWriter(8);
        var pos = Position;
        Write((ulong)0);
        Write(length);
        Write(length);
        return pos;
    }

    public long WriteDummyPointer(int length, int max)
    {
        AlignWriter(8);
        var pos = Position;
        Write((ulong)0);
        Write(length);
        Write(max);
        return pos;
    }

    public void AddIndirectDataQueue(IFF7FrozenProperty prop)
    {
        IndirectDataQueue.Enqueue(prop);
    }

    public void AddQueue(IFF7FrozenProperty prop)
    {
        WritingQueue.Enqueue(prop);
    }

    public void AddAltQueue(IFF7AltFrozenProperty prop)
    {
        AltWritingQueue.Enqueue(prop);
    }

    public void ProcessAltQueue()
    {
        while (AltWritingQueue.Count > 0)
        {
            var prop = AltWritingQueue.Dequeue();
            prop.AltWrite(this);
        }
    }

    public void ProcessWritingQueue()
    {
        while (WritingQueue.Count > 0)
        {
            var prop = WritingQueue.Dequeue();
            prop.Write(this);
        }
    }

    public void ProcessIndirectDataQueue()
    {
        while (IndirectDataQueue.Count > 0)
        {
            var prop = IndirectDataQueue.Dequeue();
            prop.Write(this);
        }
    }
}