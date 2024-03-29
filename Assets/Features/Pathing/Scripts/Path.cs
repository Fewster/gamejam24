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

    public void OnDrawGizmos()
    {
        var childCount = transform.childCount;

        for(int i = 1; i < childCount; i++) 
        {
            Gizmos.DrawLine(transform.GetChild(i - 1).position, transform.GetChild(i).position);
        }
    }
}
