using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : GameService<World>
{
    [SerializeField]
    private List<BuildZone> zones = new();

    public void Register(BuildZone zone)
    {
        zones.Add(zone);
    }
}
