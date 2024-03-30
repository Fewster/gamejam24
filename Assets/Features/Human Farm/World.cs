using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : GameService<World>
{
    public List<BuildZone> zones;

    public void Register(BuildZone zone)
    {
        zones.Add(zone);
    }
}
