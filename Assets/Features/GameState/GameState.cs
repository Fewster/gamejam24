using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : GameService<GameState>
{
    public State CurrentState { get; private set; }

    public event Action<State, State> OnStateTransition;
    
    public void Transition(State state)
    {
        if(CurrentState == state)
        {
            return;
        }

        var prev = CurrentState;
        CurrentState = state;

        OnStateTransition?.Invoke(prev,state);
    }

    [ContextMenu("Transition to Loading")]
    private void TransitionToLoading()
    {
        Transition(State.Loading);
    }

    [ContextMenu("Transition to Playing")]
    private void TransitionToPlaying()
    {
        Transition(State.Playing);
    }

    public enum State
    {
        Loading,
        Playing
    }
}
