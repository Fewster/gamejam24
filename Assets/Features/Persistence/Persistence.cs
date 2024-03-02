using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class Persistence : GameService<Persistence>
{
    public PersistenceModel Model { get; private set; } = new();

    public PersistenceProvider Provider;

    public event Action OnSaving;
    public event Action OnLoading;
    public event Action OnLoaded;

    public void Save()
    {
        // Let objects append any extra save data before saving
        OnSaving?.Invoke();

        Debug.Log($"Saving {Model.Count} properties");

        var codec = new PersistenceCodecV1();
        var data = codec.Write(Model);

        _ = Provider.Save(data);
    }

    public void Load()
    {
        // TODO: Synchronize, only allow one sync at a time, etc...

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        OnLoading?.Invoke();

        try
        {
            var data = await Provider.Load();
            if (data == null) // No data to load
            {
                // TODO: If we load and there is no data, we may accidentally wipe the local user data and lose progress!
                // What do we do in this case?

                return;
            }

            var codec = FetchCodec(data);
            if (codec == null)
            {
                // TODO: Save data is not valid, what do we do here? ...
                return;
            }

            codec.Read(data, Model);
        }
        finally
        {
            OnLoaded?.Invoke();
        }
    }

    private IPersistenceCodec FetchCodec(byte[] data)
    {
        using (var ms = new MemoryStream(data))
        {
            using (var reader = new BinaryReader(ms))
            {
                var version = reader.ReadInt32();
                switch (version)
                {
                    case (int)PersistentDataVersion.Version_1:
                        return new PersistenceCodecV1();
                    default:
                        return null;
                }
            }
        }
    }

    [ContextMenu("Force save")]
    private void ForceSave()
    {
        Save();
    }

    [ContextMenu("Force load")]
    private void ForceLoad()
    {
        Load();
    }
}

public class PersistenceModel : IEnumerable<PersistentProperty>
{
    private readonly Dictionary<PropertyIndex, PersistentProperty> properties;

    public int Count { get { return properties.Count; } }

    public PersistenceModel()
    {
        properties = new Dictionary<PropertyIndex, PersistentProperty>();
    }

    public void Clear()
    {
        properties.Clear();
    }

    public PersistentProperty GetProperty(string name, PropertyType type)
    {
        var index = new PropertyIndex(name, type);
        properties.TryGetValue(index, out var value);
        return value;
    }

    public PersistentProperty EnsureProperty(string name, PropertyType type)
    {
        var index = new PropertyIndex(name, type);
        if (!properties.TryGetValue(index, out var value))
        {
            value = CreateProperty(name, type);
            properties.Add(index, value);
        }

        return value;
    }

    private PersistentProperty CreateProperty(string name, PropertyType type)
    {
        switch (type)
        {
            case PropertyType.Double:
                return new PersistentDouble(name);
            case PropertyType.String:
                return new PersistentString(name);
            case PropertyType.Int32:
                return new PersistentInt32(name);
            default:
                throw new NotImplementedException();
        }
    }

    public IEnumerator<PersistentProperty> GetEnumerator()
    {
        return properties.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return properties.Values.GetEnumerator();
    }
}

public static class PersistenceModelExtensions
{
    public static PersistentInt32 GetInt32(this PersistenceModel model, string name)
    {
        return model.GetProperty(name, PropertyType.Int32) as PersistentInt32;
    }

    public static PersistentDouble GetDouble(this PersistenceModel model, string name)
    {
        return model.GetProperty(name, PropertyType.Double) as PersistentDouble;
    }

    public static PersistentString GetString(this PersistenceModel model, string name)
    {
        return model.GetProperty(name, PropertyType.String) as PersistentString;
    }

    public static PersistentInt32 EnsureInt32(this PersistenceModel model, string name)
    {
        return model.EnsureProperty(name, PropertyType.Int32) as PersistentInt32;
    }

    public static PersistentDouble EnsureDouble(this PersistenceModel model, string name)
    {
        return model.EnsureProperty(name, PropertyType.Double) as PersistentDouble;
    }

    public static PersistentString EnsureString(this PersistenceModel model, string name)
    {
        return model.EnsureProperty(name, PropertyType.String) as PersistentString;
    }
}

public readonly struct PropertyIndex
{
    public readonly string Name;
    public readonly PropertyType Type;

    public PropertyIndex(string name, PropertyType type)
    {
        Name = name;
        Type = type;
    }
}

public abstract class PersistentProperty
{
    public string Name { get; }
    public abstract PropertyType Type { get; }

    public PersistentProperty(string name)
    {
        Name = name;
    }
}

public class PersistentInt32 : PersistentProperty
{
    public int Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.Int32; }
    }

    public PersistentInt32(string name) : base(name) { }
}

public class PersistentDouble : PersistentProperty
{
    public double Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.Double; }
    }

    public PersistentDouble(string name) : base(name) { }
}

public class PersistentString : PersistentProperty
{
    public string Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.String; }
    }

    public PersistentString(string name) : base(name) { }
}

public enum PropertyType
{
    Double,
    String,
    Int32
}
