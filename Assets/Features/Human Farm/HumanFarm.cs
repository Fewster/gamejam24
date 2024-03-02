using Game.Framework;
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
        bank.Victims.Value += 1;
    }

    public void Consume(double count)
    {
        bank.Victims.Value += count;
    }

    [ContextMenu("Consume Debug Human")]
    private void ConsumeDebugHuman()
    {
        Consume();
    }
}
