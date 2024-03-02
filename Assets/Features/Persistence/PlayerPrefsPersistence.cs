using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Persistence/PlayerPrefs")]
public class PlayerPrefsPersistence : PersistenceProvider
{
    public string SaveKey = "PLAYER_DATA";

    public override Task<byte[]> Load()
    {
        var data = PlayerPrefs.GetString(SaveKey);
        if (data == null) // No key, no data
        {
            return Task.FromResult<byte[]>(null);
        }

        return Task.FromResult(Convert.FromBase64String(data));
    }

    public override Task Save(byte[] model)
    {
        var b64 = Convert.ToBase64String(model);
        PlayerPrefs.SetString(SaveKey, b64);

        return Task.CompletedTask;
    }

    public void WipeData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(PlayerPrefsPersistence))]
public class PlayerPrefsPersistenceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        var item = target as PlayerPrefsPersistence;
        var key = item.SaveKey;

        if (GUILayout.Button(new GUIContent("Wipe Data", $"Wipe the data saved under key: '{key}'")))
        {

            item.WipeData();
        }
    }
}

#endif
