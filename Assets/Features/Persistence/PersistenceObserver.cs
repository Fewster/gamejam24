using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PersistenceObserver : GameBehaviour
{
    private Persistence persistence;

    public UnityEvent OnBeforeLoad;
    public UnityEvent<PersistentModel> OnBeforeSave;
    public UnityEvent<PersistentModel> OnDataLoaded;

    protected override void OnSetup()
    {
        persistence = Resolver.Resolve<Persistence>();

        persistence.OnLoaded += OnLoaded;
        persistence.OnLoading += OnLoading;
        persistence.OnSaving += OnSaving;
    }

    private void OnSaving(PersistentModel model)
    {
        OnBeforeSave?.Invoke(model);

    }

    private void OnLoading()
    {
        OnBeforeLoad?.Invoke();
    }

    private void OnLoaded(PersistentModel model)
    {
        OnDataLoaded?.Invoke(model);
    }
}
