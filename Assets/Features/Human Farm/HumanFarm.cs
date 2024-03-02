using Game.Framework;
using UnityEngine;

public class HumanFarm : GameBehaviour
{
    private Bank bank;

    public double PassiveIncome = 0.0;
    public double ResourceConversionFactor = 1.0f;
    public double SoulsConversionFactor = 1.0f;

    protected override void OnSetup()
    {
        bank = Resolver.Resolve<Bank>();
    }

    public void Consume(double count)
    {
        // Keep track of total victims
        bank.Victims.Value += count;

        // TODO: We would want to split the percentage of mana/army growth here
        bank.Mana.Value += count * ResourceConversionFactor;

        //bank.Harvested.Value += count * ResourceConversionFactor;

        // TODO: Souls need to have a chance or reduced earnings somehow.
        bank.Souls.Value += count * SoulsConversionFactor;
    }

    [ContextMenu("Consume Debug Human")]
    private void ConsumeDebugHuman()
    {
        Consume(1.0);
    }

    private void Update()
    {
        Consume(PassiveIncome * Time.deltaTime);
    }
}
