using System;
using System.IO;

public class PersistenceCodecV1 : IPersistenceCodec
{
    public void Read(byte[] data, PersistenceModel model)
    {
        using (var ms = new MemoryStream(data))
        {
            using(var reader = new BinaryReader(ms))
            {
                var version = (PersistentDataVersion)reader.ReadInt32();
                if(version != PersistentDataVersion.Version_1) // Sanity check the data version
                {
                    throw new InvalidOperationException("Expected a version 1 data blob");
                }

                var count = reader.ReadInt32();
                for(int i = 0; i < count; i++)
                {
                    var type = reader.ReadInt32();
                    switch (type)
                    {
                        case (int)PropertyType.Double:
                            {
                                var name = reader.ReadString();
                                var value = reader.ReadDouble();

                                var property = model.EnsureDouble(name);
                                property.Value = value;
                            }
                            break;
                        case (int)PropertyType.Int32:
                            {
                                var name = reader.ReadString();
                                var value = reader.ReadInt32();

                                var property = model.EnsureInt32(name);
                                property.Value = value;
                            }
                            break;
                        case (int)PropertyType.String:
                            {
                                var name = reader.ReadString();
                                var value = reader.ReadString();

                                var property = model.EnsureString(name);
                                property.Value = value;
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Unknown data type");
                    }
                }
            }
        }
    }

    public byte[] Write(PersistenceModel model)
    {
        using (var ms = new MemoryStream())
        {
            using(var writer = new BinaryWriter(ms))
            {
                // Header
                writer.Write((int)PersistentDataVersion.Version_1);
                writer.Write((int)model.Count);

                foreach (var property in model)
                {
                    switch (property.Type)
                    {
                        case PropertyType.Double:
                            {
                                var prop = property as PersistentDouble;
                                writer.Write((int)PropertyType.Double);
                                writer.Write(prop.Name);
                                writer.Write(prop.Value);
                            }
                            break;
                        case PropertyType.String:
                            {
                                var prop = property as PersistentString;
                                writer.Write((int)PropertyType.String);
                                writer.Write(prop.Name);
                                writer.Write(prop.Value);
                            }
                            break;
                        case PropertyType.Int32:
                            {
                                var prop = property as PersistentInt32;
                                writer.Write((int)PropertyType.Int32);
                                writer.Write(prop.Name);
                                writer.Write(prop.Value);
                            }
                            break;
                    }
                }

                return ms.ToArray();
            }
        }
    }
}