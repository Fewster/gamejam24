using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class Persistence : GameService<Persistence>
{
    public PersistenceProvider Provider;

    public event Action<PersistentModel> OnSaving;
    public event Action<PersistentModel> OnLoaded;
    public event Action OnLoading;

    public void Save()
    {
        var model = new PersistentModel();

        // Let objects append any extra save data before saving
        OnSaving?.Invoke(model);

       // Debug.Log($"Saving {model.Count} containers");

        foreach(var container in model)
        {
            Debug.Log($"Saving Container {container.Name}");
            foreach(var property in container)
            {
                Debug.Log($"Saving Property {property.Name} {property.Type}");
            }
        }

        var codec = new PersistenceCodecV1();
        var data = codec.Write(model);

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

        var model = new PersistentModel();

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

            codec.Read(data, model);
        }
        finally
        {
            OnLoaded?.Invoke(model);
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

internal static class PropertyFactory
{
    public static PersistentProperty Create(string name, PropertyType type)
    {
        switch (type)
        {
            case PropertyType.Double:
                return new DoubleProperty(name);
            case PropertyType.String:
                return new StringProperty(name);
            case PropertyType.Int32:
                return new Int32Property(name);
            case PropertyType.Bool:
                return new BoolProperty(name);
            default:
                throw new NotImplementedException();
        }
    }
}

public interface IPropertyFactory
{
    PersistentProperty GetProperty(string name, PropertyType type);
    PersistentProperty EnsureProperty(string name, PropertyType type);
}

public class PersistentModel : IEnumerable<PersistentContainer>
{
    private readonly Dictionary<string, PersistentContainer> containers;

    public int Count { get { return containers.Count; } }

    public PersistentModel()
    {
        containers = new Dictionary<string, PersistentContainer>();
    }

    public PersistentContainer GetContainer(string name)
    {
        containers.TryGetValue(name, out var container);
        return container;
    }

    public PersistentContainer EnsureContainer(string name)
    {
        if(!containers.TryGetValue(name, out var container))
        {
            container = new PersistentContainer(name);
            containers.Add(name, container);
        }

        return container;
    }

    public PersistentContainer CreateContainer(string name)
    {
        if (containers.ContainsKey(name))
        {
            throw new InvalidOperationException("An object already exists with the given name");
        }

        var instance = new PersistentContainer(name);
        containers[name] = instance;
        return instance;
    }

    public IEnumerator<PersistentContainer> GetEnumerator()
    {
        return containers.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return containers.Values.GetEnumerator();
    }
}

public class PersistentContainer : 
    IEnumerable<PersistentProperty>,
    IPropertyFactory
{
    private readonly Dictionary<PropertyIndex, PersistentProperty> properties;

    public string Name { get; private set; }
    public int Count { get { return properties.Count; } }

    internal PersistentContainer(string name)
    {
        Name = name;
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
            value = PropertyFactory.Create(name, type);
            properties.Add(index, value);
        }

        return value;
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

public static class PropertyFactoryExtensions
{
    public static Int32Property GetInt32(this IPropertyFactory factory, string name)
    {
        return factory.GetProperty(name, PropertyType.Int32) as Int32Property;
    }

    public static DoubleProperty GetDouble(this IPropertyFactory factory, string name)
    {
        return factory.GetProperty(name, PropertyType.Double) as DoubleProperty;
    }

    public static StringProperty GetString(this IPropertyFactory factory, string name)
    {
        return factory.GetProperty(name, PropertyType.String) as StringProperty;
    }

    public static BoolProperty GetBool(this IPropertyFactory factory, string name)
    {
        return factory.GetProperty(name, PropertyType.Bool) as BoolProperty;
    }

    public static Int32Property EnsureInt32(this IPropertyFactory factory, string name)
    {
        return factory.EnsureProperty(name, PropertyType.Int32) as Int32Property;
    }

    public static DoubleProperty EnsureDouble(this IPropertyFactory factory, string name)
    {
        return factory.EnsureProperty(name, PropertyType.Double) as DoubleProperty;
    }

    public static StringProperty EnsureString(this IPropertyFactory factory, string name)
    {
        return factory.EnsureProperty(name, PropertyType.String) as StringProperty;
    }

    public static BoolProperty EnsureBool(this IPropertyFactory factory, string name)
    {
        return factory.EnsureProperty(name, PropertyType.Bool) as BoolProperty;
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

public class Int32Property : PersistentProperty
{
    public int Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.Int32; }
    }

    public Int32Property(string name) : base(name) { }
}

public class DoubleProperty : PersistentProperty
{
    public double Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.Double; }
    }

    public DoubleProperty(string name) : base(name) { }
}

public class StringProperty : PersistentProperty
{
    public string Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.String; }
    }

    public StringProperty(string name) : base(name) { }
}

public class BoolProperty : PersistentProperty
{
    public bool Value { get; set; }

    public override PropertyType Type
    {
        get { return PropertyType.Bool; }
    }

    public BoolProperty(string name) : base(name) { }
}

public enum PropertyType
{
    Double,
    String,
    Int32,
    Bool
}
