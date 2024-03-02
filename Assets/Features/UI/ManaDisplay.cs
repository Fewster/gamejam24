using Game.Framework;
using UnityEngine;
using UnityEngine.Events;

public class ManaDisplay : GameBehaviour
{
    private Bank bank;
    private ulong lastVersion;
    private float displayTime;

    public float DisplayRate = 0.25f;
    public UnityEvent<string> SetContent;

    protected override void OnSetup()
    {
        bank = Resolver.Resolve<Bank>();
        Synchronize(bank.Mana.Value);
    }

    private void Update()
    {
        var now = Time.time;
        if(now > displayTime)
        {
            var stat = bank.Mana;
            if (stat.Version != lastVersion)
            {
                lastVersion = stat.Version;
                displayTime = now + DisplayRate;

                Synchronize(stat.Value);
            }
        }
    }

    private void Synchronize(double value)
    {
        // TODO: We can handle formatting here ...

        var num = LargeNumberUtility.FormatLargeNumber(value);
        SetContent?.Invoke(num);

        //// HORRIBLE APPROACH, DO THIS BETTER
        //var log = System.Math.Log10(value);
        //int flr = Mathf.FloorToInt((float)log);

        //switch (flr)
        //{
        //    default:
        //        break;
        //}

        //var text = $"{string.Format("{0:0.00} ", value)} {flr}";
        //SetContent?.Invoke(text);
    }

   
}

public static class LargeNumberUtility
{
    private static string[] table = new string[]
    {
        "",
        "K",
        "M",
        "B",
        "T",
        "q",
        "Q",
        "s",
        "S",
        "O",
        "N",
        "d",
        "U",
        "D"
        // TODO: More...
    };

    public static string FormatLargeNumber(double value)
    {
        var idx = 0;
        while (value >= 1000d)
        {
            idx++;
            value /= 1000d;
        }

        return value.ToString("F2") + table[idx];
    }
}