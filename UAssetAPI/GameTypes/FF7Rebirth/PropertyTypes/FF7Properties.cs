using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UAssetAPI.GameTypes.FF7Rebirth.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.GameTypes.FF7Rebirth.PropertyTypes;

public interface IFF7FrozenProperty
{
    public virtual void Write(FMemoryMappedImageWriter Ar) { }
}

public interface IFF7AltFrozenProperty
{
    public virtual void AltWrite(FMemoryMappedImageWriter Ar) { }
}

public enum FF7PropertyType : int
{
    BoolProperty = 1,
    ByteProperty = 2,
    Int8Property = 3,
    UInt16Property = 4,
    Int16Property = 5,
    UIntProperty = 6,
    IntProperty = 7,
    Int64Property = 8,
    FloatProperty = 9,
    StrProperty = 10,
    NameProperty = 11,
}

public static class PropUtils
{
    public static int GetPropAlign(FF7PropertyType type)
    {
        return type switch
        {
            FF7PropertyType.BoolProperty => 1,
            FF7PropertyType.ByteProperty => 1,
            FF7PropertyType.Int8Property => 1,
            FF7PropertyType.UInt16Property => 2,
            FF7PropertyType.Int16Property => 2,
            FF7PropertyType.StrProperty => 8,
            _ => 4,
        };
    }

    public static string GetPropType(FF7PropertyType type)
    {
        return type switch
        {
            FF7PropertyType.BoolProperty => "FF7BoolProperty",
            FF7PropertyType.ByteProperty => "FF7ByteProperty",
            FF7PropertyType.Int8Property => "FF7Int8Property",
            FF7PropertyType.UInt16Property => "FF7UInt16Property",
            FF7PropertyType.Int16Property => "FF7Int16Property",
            FF7PropertyType.UIntProperty => "FF7UIntProperty",
            FF7PropertyType.IntProperty => "FF7IntProperty",
            FF7PropertyType.Int64Property => "FF7Int64Property",
            FF7PropertyType.FloatProperty => "FF7FloatProperty",
            FF7PropertyType.StrProperty => "FF7StrProperty",
            FF7PropertyType.NameProperty => "FF7NameProperty",
            _ => "UnknownProperty",
        };
    }
}

public class FF7ArrayView(byte[] value, long pointerOffset) : IFF7FrozenProperty
{
    public byte[] Value = value;
    public long PointerOffset = pointerOffset;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        var hash = BitConverter.ToUInt64(SHA1.HashData(Value), 0);
        Ar.Arrays[hash] = Ar.Position;
        Ar.AddPointer(PointerOffset, Ar.Position);
        Ar.Write(Value);
    }
}

public class FF7BoolProperty : BoolPropertyData, IFF7FrozenProperty
{
    public FF7BoolProperty() { }
    public FF7BoolProperty(FName name) : base(name) { }
    public FF7BoolProperty(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadBoolean();
    }

    private static readonly FString CurrentPropertyType = new FString("FF7BoolProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7ByteProperty : BytePropertyData, IFF7FrozenProperty
{
    public FF7ByteProperty() { }
    public FF7ByteProperty(FName name) : base(name) { }
    public FF7ByteProperty(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadByte();
        ByteType = BytePropertyType.Byte;
    }

    private static readonly FString CurrentPropertyType = new FString("FF7ByteProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7Int8Property : Int8PropertyData, IFF7FrozenProperty
{
    public FF7Int8Property() { }
    public FF7Int8Property(FName name) : base(name) { }
    public FF7Int8Property(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadSByte();
    }

    private static readonly FString CurrentPropertyType = new FString("FF7Int8Property");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7UInt16Property : UInt16PropertyData, IFF7FrozenProperty
{
    public FF7UInt16Property() { }
    public FF7UInt16Property(FName name) : base(name) { }
    public FF7UInt16Property(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadUInt16();
        //Ar.Align(2);
    }

    private static readonly FString CurrentPropertyType = new FString("FF7UInt16Property");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7Int16Property : Int16PropertyData, IFF7FrozenProperty
{
    public FF7Int16Property() { }
    public FF7Int16Property(FName name) : base(name) { }
    public FF7Int16Property(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadInt16();
        //Ar.Align(2);
    }

    private static readonly FString CurrentPropertyType = new FString("FF7Int16Property");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7UIntProperty : UInt32PropertyData, IFF7FrozenProperty
{
    public FF7UIntProperty() { }
    public FF7UIntProperty(FName name) : base(name) { }
    public FF7UIntProperty(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadUInt32();
    }

    private static readonly FString CurrentPropertyType = new FString("FF7UIntProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7IntProperty : IntPropertyData, IFF7FrozenProperty
{
    public FF7IntProperty() { }
    public FF7IntProperty(FName name) : base(name) { }
    public FF7IntProperty(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadInt32();
    }

    private static readonly FString CurrentPropertyType = new FString("FF7IntProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7Int64Property : Int64PropertyData, IFF7FrozenProperty
{
    public FF7Int64Property() { }
    public FF7Int64Property(FName name) : base(name) { }
    public FF7Int64Property(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadInt64();
        Ar.Align(8);
    }

    private static readonly FString CurrentPropertyType = new FString("FF7Int64Property");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7FloatProperty : FloatPropertyData, IFF7FrozenProperty
{
    public FF7FloatProperty() { }
    public FF7FloatProperty(FName name) : base(name) { }
    public FF7FloatProperty(FMemoryMappedImageArchive Ar)
    {
        Value = Ar.ReadSingle();
        Ar.Align(4);
    }

    private static readonly FString CurrentPropertyType = new FString("FF7FloatProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.Write(Value);
    }
}

public class FF7String : FString, IFF7FrozenProperty, IFF7AltFrozenProperty
{
    public long PointerOffset;

    public FF7String() { }
    public FF7String(string value, Encoding encoding = null) : base(value, encoding) { }
    public FF7String(FString value, long pointerOffset)
    {
        Value = value.Value;
        Encoding = value.Encoding;
        PointerOffset = pointerOffset;
    }

    public void Write(FMemoryMappedImageWriter Ar)
    {
        byte[] actualStrData = Encoding.GetBytes(Value+"\0");
        var hash = BitConverter.ToUInt64(SHA1.HashData(actualStrData), 0);

        if (Ar.Arrays.TryGetValue(hash, out var arroffset))
        {
            Ar.AddPointer(PointerOffset, arroffset);
        }
        else
        {
            Ar.AlignWriter(2);
            Ar.AddPointer(PointerOffset, Ar.Position);
            Ar.Arrays[hash] = Ar.Position;
            Ar.Write(actualStrData);
        }
    }

    public void AltWrite(FMemoryMappedImageWriter Ar) => Write(Ar);
}

public class FF7StrProperty : StrPropertyData, IFF7FrozenProperty, IFF7AltFrozenProperty
{
    public FF7StrProperty() { }
    public FF7StrProperty(FName name) : base(name) { }
    public FF7StrProperty(FMemoryMappedImageArchive Ar)
    {
        Ar.Align(8);
        Value = Ar.ReadFString();
    }

    private static readonly FString CurrentPropertyType = new FString("FF7StrProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        if (Value == null)
        {
            Ar.WriteDummyPointer(0);
            return;
        }
        var pointerOffset = Ar.WriteDummyPointer(Value.Value.Length + 1);
        Ar.AddIndirectDataQueue(new FF7String(Value, pointerOffset));
    }

    public void AltWrite(FMemoryMappedImageWriter Ar)
    {
        if (Value == null)
        {
            Ar.WriteDummyPointer(0);
            return;
        }
        var pointerOffset = Ar.WriteDummyPointer(Value.Value.Length + 1);
        Ar.AddAltQueue(new FF7String(Value, pointerOffset));
    }
}

public class FF7NameProperty : NamePropertyData, IFF7FrozenProperty
{
    public FF7NameProperty() { }
    public FF7NameProperty(FName name) : base(name) { }
    public FF7NameProperty(FMemoryMappedImageArchive Ar)
    {
        Ar.Align(4);
        Value = Ar.ReadFName();
    }

    private static readonly FString CurrentPropertyType = new FString("FF7NameProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.WriteFName(Value);
    }
}

public class FF7InnerArrayProperty : IFF7FrozenProperty
{
    public PropertyData[] Value;
    public FF7PropertyType ElementType;
    public long PointerOffset;

    public FF7InnerArrayProperty() { }
    public FF7InnerArrayProperty(PropertyData[] value, long pointerOffset, FF7PropertyType elementType)
    {
        Value = value;
        PointerOffset = pointerOffset;
        ElementType = elementType;
    }
    
    public void Write(FMemoryMappedImageWriter Ar)
    {
        var tempAr = new FMemoryMappedImageWriter(new MemoryStream(), true);
        for (var i = 0; i < Value.Length; i++)
        {
            tempAr.AddQueue(Value[i] as IFF7FrozenProperty);
        }
        tempAr.Write();
        tempAr.Position = 0;
        var hash = BitConverter.ToUInt64(SHA1.Create().ComputeHash(tempAr.BaseStream), 0);

        // don't reuse string arrays
        if (ElementType is not FF7PropertyType.StrProperty && Ar.Arrays.TryGetValue(hash, out var arroffset))
        {
            Ar.AddPointer(PointerOffset, arroffset);
            return;
        }
       
        // need to add align here based on innertype
        Ar.AlignWriter(PropUtils.GetPropAlign(ElementType));
        Ar.AddPointer(PointerOffset, Ar.Position);
        Ar.Arrays[hash] = Ar.Position;

        if(ElementType is not FF7PropertyType.NameProperty and not FF7PropertyType.StrProperty)
        {
            //copy from tempAr to mainAr
            tempAr.Position = 0;
            tempAr.BaseStream.CopyTo(Ar.BaseStream);
        }
        else if(ElementType is FF7PropertyType.NameProperty)
        {
            var saved = (int)Ar.Position;
            foreach (NamePropertyData prop in Value)
            {
                if (prop.Value is not null)
                    Ar.AddName(prop.Value, saved);
                saved += 8;
            }

            Ar.BaseStream.SetLength(Ar.BaseStream.Length + Value.Length * 8);
            Ar.Position += Value.Length * 8;
        }
        else
        {
            for (var i = 0; i < Value.Length; i++)
            {
                Ar.AddAltQueue(Value[i] as IFF7AltFrozenProperty);
            }
            Ar.ProcessAltQueue();
        }
    }
}

public class FF7ArrayProperty : ArrayPropertyData, IFF7FrozenProperty
{
    public FF7PropertyType ElementType;
    
    public FF7ArrayProperty() { }
    public FF7ArrayProperty(FName name) : base(name) { }
    public FF7ArrayProperty(FMemoryMappedImageArchive Ar, FF7Property property)
    {
        ElementType = property.UnderlyingType;
        Ar.Align(8);
        var offset = Ar.Offset;
        var align = PropUtils.GetPropAlign(ElementType);

        Value = Ar.ReadArray(() => Ar.ReadPropertyData(property), align);
        ArrayType = FName.DefineDummy(Ar.Asset, PropUtils.GetPropType(property.UnderlyingType));
        Offset = offset;
        Name = property.Name;
    }

    private static readonly FString CurrentPropertyType = new FString("FF7ArrayProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        if (Value == null || Value.Length == 0)
        {
            Ar.WriteDummyPointer(0);
            return;
        }
        var pointerOffset = Ar.WriteDummyPointer(Value.Length);
        Ar.AddIndirectDataQueue(new FF7InnerArrayProperty(Value, pointerOffset, ElementType));
    }
}

public class FF7StructProperty : StructPropertyData, IFF7FrozenProperty
{
    public FF7StructProperty() { }
    public FF7StructProperty(FName name) : base(name) { }
    public FF7StructProperty(List<PropertyData> properties)
    {
        Value = properties;
    }

    private static readonly FString CurrentPropertyType = new FString("FF7StructProperty");
    public override FString PropertyType => CurrentPropertyType;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        var align = PropUtils.GetPropAlign(Ar.StructDefinition[0].UnderlyingType);
        Ar.AddQueue(new FF7RealignProperty(align));
        for (var i = 0; i < Value.Count; i++)
        {
            Ar.AddQueue(Value[i] as IFF7FrozenProperty);
        }
    }
}

public class FF7RealignProperty(int align) : IFF7FrozenProperty
{
    public int Align = align;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.AlignWriter(Align);
    }
}

public class FF7OffsetProperty(long pointerOffset) : IFF7FrozenProperty
{
    public long PointerOffset = pointerOffset;

    public void Write(FMemoryMappedImageWriter Ar)
    {
        Ar.AddPointer(PointerOffset, Ar.Position);
    }
}