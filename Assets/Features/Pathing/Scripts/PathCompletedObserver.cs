using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PathCompletedObserver : GameBehaviour
{
    public UnityEvent OnPathComplete;

    public int PathIndex;

    protected override void OnSetup()
    {
        base.OnSetup();
        var agentService = Resolver.Resolve<AgentService>();
        agentService.OnRouteComplete.AddListener(PathCompleted);
    }

    void PathCompleted(RouteCompleteArgs routeInfo)
    {
        if(PathIndex == routeInfo.RouteId)
        {
            OnPathComplete.Invoke();
        }
    }
}
