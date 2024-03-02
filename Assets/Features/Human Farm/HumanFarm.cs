using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFarm : GameBehaviour
{
    private Bank bank;

    protected override void OnSetup()
    {
        bank = Resolver.Resolve<Bank>();
    }

    public void Consume()
    {
        // TODO: Multipliers should be applied here

        bank.Victims.Value += 1;

        OnConsumed();
    }

    protected virtual void OnConsumed() { }

    [ContextMenu("Consume Debug Human")]
    private void ConsumeDebugHuman()
    {
        Consume();
    }
}
