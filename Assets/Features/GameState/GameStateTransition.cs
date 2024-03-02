using Game.Framework;
using UnityEngine.Events;

public class GameStateTransition : GameBehaviour
{
    private GameState state;

    public GameState.State From;
    public GameState.State To;

    public UnityEvent OnTransition;

    protected override void OnSetup()
    {
        state = Resolver.Resolve<GameState>();
        state.OnStateTransition += OnStateTransition;
    }

    protected override void OnCleanup()
    {
        if (state != null)
        {
            state.OnStateTransition -= OnStateTransition;
        }
    }

    private void OnStateTransition(GameState.State from, GameState.State to)
    {
        if(from == From && to == To)
        {
            OnTransition?.Invoke();
        }
    }
}
