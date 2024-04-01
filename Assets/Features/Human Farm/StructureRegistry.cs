using Game.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureRegistry : GameService<StructureRegistry>
{
    [SerializeField]
    private List<Structure> entries = new();

    public Structure GetPrefab(string name)
    {
        foreach(var entry in entries)
        {
            if(entry.Type == name)
            {
                return entry;
            }
        }

        return null;
    }
}
