using Game.Framework;
using UnityEngine.Events;

public class SoulDisplay : GameBehaviour
{
    private Bank bank;
    private ulong lastVersion;

    public UnityEvent<string> SetContent;

    protected override void OnSetup()
    {
        bank = Resolver.Resolve<Bank>();
        Synchronize(bank.Souls.Value);
    }

    private void Update()
    {
        var stat = bank.Souls;
        if (stat.Version != lastVersion)
        {
            lastVersion = stat.Version;
            Synchronize(stat.Value);
        }
    }

    private void Synchronize(double value)
    {
        // TODO: We can handle formatting here ...

        var text = $"{string.Format("{0:0.##}", value)}";
        SetContent?.Invoke(text);
    }
}
