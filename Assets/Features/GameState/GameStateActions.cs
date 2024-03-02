using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateActions : GameBehaviour
{
    private GameState gameState;

    protected override void OnSetup()
    {
        gameState = Resolver.Resolve<GameState>();
    }

    public void TransitionToLoading()
    {
        gameState.Transition(GameState.State.Loading);
    }

    public void TransitionToPlaying()
    {
        gameState.Transition(GameState.State.Playing);
    }
}
