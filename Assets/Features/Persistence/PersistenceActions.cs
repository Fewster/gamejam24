using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistenceActions : GameBehaviour
{
    private Persistence persistence;

    protected override void OnSetup()
    {
        persistence = Resolver.Resolve<Persistence>();
    }

    public void Save()
    {
        persistence?.Save(); 
    }

    public void Load()
    {
        persistence?.Load();
    }
}
