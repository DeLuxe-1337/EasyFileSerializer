public interface FileSerializer
{
    void WriteToStream(BinaryWriter writer)
    {
        Type type = GetType();
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            object? value = field.GetValue(this);
            WriteValueToStream(writer, field.FieldType, value!);
        }
    }

    object ReadFromStream(BinaryReader reader)
    {
        Type type = GetType();
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            object result = ReadValueFromStream(reader, field.FieldType);
            field.SetValue(this, result);
        }

        return this;
    }

    private static void WriteValueToStream(BinaryWriter writer, Type type, object value)
    {
        TypeCode typeCode = Type.GetTypeCode(type);
        switch (typeCode)
        {
            case TypeCode.Boolean:
                writer.Write((bool)value);
                break;
            case TypeCode.Byte:
                writer.Write((byte)value);
                break;
            case TypeCode.SByte:
                writer.Write((sbyte)value);
                break;
            case TypeCode.Char:
                writer.Write((char)value);
                break;
            case TypeCode.Int16:
                writer.Write((short)value);
                break;
            case TypeCode.UInt16:
                writer.Write((ushort)value);
                break;
            case TypeCode.Int32:
                writer.Write((int)value);
                break;
            case TypeCode.UInt32:
                writer.Write((uint)value);
                break;
            case TypeCode.Int64:
                writer.Write((long)value);
                break;
            case TypeCode.UInt64:
                writer.Write((ulong)value);
                break;
            case TypeCode.Single:
                writer.Write((float)value);
                break;
            case TypeCode.Double:
                writer.Write((double)value);
                break;
            case TypeCode.String:
                writer.Write((string)value);
                break;
            default:
                {
                    if (type.IsArray)
                    {
                        Type? elementType = type.GetElementType();
                        int length = ((Array)value).Length;
                        writer.Write(length);
                        for (int i = 0; i < length; i++)
                        {
                            WriteValueToStream(writer, elementType!, ((Array)value).GetValue(i));
                        }
                    }
                    else
                    {
                        // check if value implements FileSerializer interface
                        if (value.GetType().GetInterfaces().Contains(typeof(FileSerializer)))
                        {
                            (value as FileSerializer).WriteToStream(writer);
                        }
                        else
                        {
                            // if it's not an array or a FileSerializer, throw an exception
                            throw new ArgumentException($"Unsupported field type: {type}");
                        }
                    }
                    break;
                }
        }
    }
    private static object ReadValueFromStream(BinaryReader reader, Type type)
    {
        if (type.GetInterfaces().Contains(typeof(FileSerializer)))
        {
            object value = Activator.CreateInstance(type);
            _ = (value as FileSerializer).ReadFromStream(reader);
            return value;
        }
        else if (type.IsArray)
        {
            Type? elementType = type.GetElementType();
            int length = reader.ReadInt32();
            Array array = Array.CreateInstance(elementType!, length);
            for (int i = 0; i < length; i++)
            {
                array.SetValue(ReadValueFromStream(reader, elementType!), i);
            }
            return array;
        }

        TypeCode typeCode = Type.GetTypeCode(type);
        return typeCode switch
        {
            TypeCode.Boolean => reader.ReadBoolean(),
            TypeCode.Byte => reader.ReadByte(),
            TypeCode.SByte => reader.ReadSByte(),
            TypeCode.Char => reader.ReadChar(),
            TypeCode.Int16 => reader.ReadInt16(),
            TypeCode.UInt16 => reader.ReadUInt16(),
            TypeCode.Int32 => reader.ReadInt32(),
            TypeCode.UInt32 => reader.ReadUInt32(),
            TypeCode.Int64 => reader.ReadInt64(),
            TypeCode.UInt64 => reader.ReadUInt64(),
            TypeCode.Single => reader.ReadSingle(),
            TypeCode.Double => reader.ReadDouble(),
            TypeCode.String => reader.ReadString(),
            _ => throw new ArgumentException($"Unsupported field type: {type}"),
        };
    }
}