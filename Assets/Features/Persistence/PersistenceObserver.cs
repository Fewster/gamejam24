using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PersistenceObserver : GameBehaviour
{
    private Persistence persistence;

    public UnityEvent OnBeforeLoad;
    public UnityEvent OnBeforeSave;
    public UnityEvent OnDataLoaded;

    protected override void OnSetup()
    {
        persistence = Resolver.Resolve<Persistence>();

        persistence.OnLoaded += OnLoaded;
        persistence.OnLoading += OnLoading;
        persistence.OnSaving += OnSaving;
    }

    private void OnSaving()
    {
        OnBeforeSave?.Invoke();

    }

    private void OnLoading()
    {
        OnBeforeLoad?.Invoke();
    }

    private void OnLoaded()
    {
        OnDataLoaded?.Invoke();
    }
}
