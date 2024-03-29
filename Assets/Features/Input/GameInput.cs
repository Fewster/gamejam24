using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameInput : GameService<GameInput>
{
    private AgentService agentService;  

    [Tooltip("The maximum number of taps which are accepted per second.")]
    public int TapsPerSecond = 10;

    protected override void OnSetup()
    {
        agentService = Resolver.Resolve<AgentService>();
    }

    public void Tap()
    {
        agentService.SpawnAgents(1);
    }
}
