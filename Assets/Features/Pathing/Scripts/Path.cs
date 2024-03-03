using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : GameBehaviour
{
    public int PathIndex = 0;

    protected override void OnSetup()
    {
        base.OnSetup();
        var service = Resolver.Resolve<AgentService>();
        service.RegisterPath(PathIndex, this);
    }
}
