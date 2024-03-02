using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatically discovers all <see cref="IPersistent"/> and notifies them whenever a save/load occurs.
/// </summary>
public class NotifyPersistenceEvents : GameBehaviour
{
    private Persistence persistence;

    protected override void OnSetup()
    {
        persistence = Resolver.Resolve<Persistence>();
        persistence.OnLoaded += OnLoaded;
        persistence.OnSaving += OnSaving;
    }

    protected override void OnCleanup()
    {
        if (persistence != null)
        {
            persistence.OnLoaded -= OnLoaded;
            persistence.OnSaving -= OnSaving;
        }
    }

    private void OnSaving()
    {
        var objects = GetComponents<IPersistent>();
        foreach(var obj in objects)
        {
            obj.Save(persistence.Model);
        }
    }

    private void OnLoaded()
    {
        var objects = GetComponents<IPersistent>();
        foreach (var obj in objects)
        {
            obj.Load(persistence.Model);
        }
    }
}
