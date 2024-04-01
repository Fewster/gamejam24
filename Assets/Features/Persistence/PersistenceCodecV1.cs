using System;
using System.IO;

public class PersistenceCodecV1 : IPersistenceCodec
{
    public void Read(byte[] data, PersistentModel model)
    {
        using (var ms = new MemoryStream(data))
        {
            using (var reader = new BinaryReader(ms))
            {
                var version = (PersistentDataVersion)reader.ReadInt32();
                if (version != PersistentDataVersion.Version_1) // Sanity check the data version
                {
                    throw new InvalidOperationException("Expected a version 1 data blob");
                }

                var objectCount = reader.ReadInt32();
                for(int i = 0; i < objectCount; i++)
                {
                    var name = reader.ReadString();
                    var container = model.CreateContainer(name);

                    ReadContainer(reader, container);
                }
            }
        }
    }

    public byte[] Write(PersistentModel model)
    {
        using (var ms = new MemoryStream())
        {
            using (var writer = new BinaryWriter(ms))
            {
                // Header
                writer.Write((int)PersistentDataVersion.Version_1);
                writer.Write((int)model.Count);

                foreach (var container in model)
                {
                    WriteContainer(writer, container);
                }
            }

            return ms.ToArray();
        }
    }

    private void ReadContainer(BinaryReader reader, PersistentContainer container)
    {
        var count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var type = reader.ReadInt32();
            switch (type)
            {
                case (int)PropertyType.Double:
                    {
                        var name = reader.ReadString();
                        var value = reader.ReadDouble();

                        var property = container.EnsureDouble(name);
                        property.Value = value;
                    }
                    break;
                case (int)PropertyType.Int32:
                    {
                        var name = reader.ReadString();
                        var value = reader.ReadInt32();

                        var property = container.EnsureInt32(name);
                        property.Value = value;
                    }
                    break;
                case (int)PropertyType.String:
                    {
                        var name = reader.ReadString();
                        var value = reader.ReadString();

                        var property = container.EnsureString(name);
                        property.Value = value;
                    }
                    break;
                case (int)PropertyType.Bool:
                    {
                        var name = reader.ReadString();
                        var value = reader.ReadBoolean();

                        var property = container.EnsureBool(name);
                        property.Value = value;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown data type");
            }
        }
    }

    private void WriteContainer(BinaryWriter writer, PersistentContainer container)
    {
        writer.Write(container.Name);
        writer.Write(container.Count);

        foreach (var property in container)
        {
            switch (property.Type)
            {
                case PropertyType.Double:
                    {
                        var prop = property as DoubleProperty;
                        writer.Write((int)PropertyType.Double);
                        writer.Write(prop.Name);
                        writer.Write(prop.Value);
                    }
                    break;
                case PropertyType.String:
                    {
                        var prop = property as StringProperty;
                        writer.Write((int)PropertyType.String);
                        writer.Write(prop.Name);
                        writer.Write(prop.Value);
                    }
                    break;
                case PropertyType.Int32:
                    {
                        var prop = property as Int32Property;
                        writer.Write((int)PropertyType.Int32);
                        writer.Write(prop.Name);
                        writer.Write(prop.Value);
                    }
                    break;
                case PropertyType.Bool:
                    {
                        var prop = property as BoolProperty;
                        writer.Write((int)PropertyType.Bool);
                        writer.Write(prop.Name);
                        writer.Write(prop.Value);
                    }
                    break;
            }
        }
    }
}