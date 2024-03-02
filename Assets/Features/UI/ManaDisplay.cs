using Game.Framework;
using UnityEngine.Events;

public class ManaDisplay : GameBehaviour
{
    private Bank bank;
    private ulong lastVersion;

    public UnityEvent<string> SetContent;

    protected override void OnSetup()
    {
        bank = Resolver.Resolve<Bank>();
        Synchronize(bank.Mana.Value);
    }

    private void Update()
    {
        var stat = bank.Mana;
        if(stat.Version != lastVersion)
        {
            lastVersion = stat.Version;
            Synchronize(stat.Value);
        }
    }

    private void Synchronize(double value)
    {
        // TODO: We can handle formatting here ...

        var text = $"{value}";
        SetContent?.Invoke(text);
    }
}
