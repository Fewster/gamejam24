using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temple : GameService<Temple>
{
    private Bank bank;

    protected override void OnSetup()
    {
        bank = Resolver.Resolve<Bank>();

    }

    private void Update()
    {
        
    }
}
