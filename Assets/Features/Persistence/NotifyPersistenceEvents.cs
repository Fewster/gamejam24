using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    private void OnSaving(PersistentModel model)
    {
        var objects = GetComponents<IPersistent>();
        foreach(var obj in objects)
        {
            obj.Save(model);
        }
    }

    private void OnLoaded(PersistentModel model)
    {
        var objects = GetComponents<IPersistent>();
        foreach (var obj in objects)
        {
            obj.Load(model);
        }
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(NotifyPersistenceEvents))]
public class NotifyPersistenceEventsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Automatically notifies Save/Load events on all components of this object.", MessageType.Info);
    }
}

#endif