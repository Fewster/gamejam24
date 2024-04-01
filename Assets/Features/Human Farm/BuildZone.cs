using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildZone : GameBehaviour, IPersistent
{
    private StructureRegistry registry;

    public string Identifier;
    public Structure Structure;

    public World World { get; private set; }

    protected override void OnSetup()
    {
        World = Resolver.Resolve<World>();
        World.Register(this);

        registry = Resolver.Resolve<StructureRegistry>();
    }

    public bool IsBuilt()
    {
        return Structure != null;
    }

    public void Load(PersistentModel model)
    {
        var container = model.GetContainer(Identifier);
        if (container == null)
        {
            ClearStructure();
            return;
        }

        var typeProperty = container.GetString("TYPE");
        if (typeProperty == null)
        {
            ClearStructure();
            return;
        }

        ChangeStructure(typeProperty.Value);

        if (Structure != null)
        {
            Structure.Load(container);
        }
    }

    public void Save(PersistentModel model)
    {
        if (Structure == null)
        {
            return;
        }

        var container = model.EnsureContainer(Identifier);
        var typeProperty = container.EnsureString("TYPE");

        typeProperty.Value = Structure.Type;
        Structure.Save(container);
    }

    private void ChangeStructure(string type)
    {
        if (Structure.Type == type)
        {
            return;
        }

        ClearStructure();

        var prefab = registry.GetPrefab(type);
        if (prefab == null)
        {
            return;
        }

        var instance = Instantiate<Structure>(prefab);
        instance.transform.parent = transform;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Structure = instance;
    }

    private void ClearStructure()
    {
        if (Structure != null)
        {
            Destroy(Structure.gameObject);
            Structure = null;
        }
    }
}
