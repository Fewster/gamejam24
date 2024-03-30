using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildZone : GameBehaviour
{
    public string Identifier;
    public Structure Structure;

    public World World { get; private set; }

    protected override void OnSetup()
    {
        World = Resolver.Resolve<World>();
        World.Register(this);
    }

    public bool IsBuilt()
    {
        return Structure != null;
    }
}
