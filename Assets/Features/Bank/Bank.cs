using Game.Framework;
using System;
using UnityEngine;

public class Bank : 
    GameService<Bank>,
    IPersistent
{
    /// <summary>
    /// The total souls that the player has.
    /// 'Premium' currency.
    /// </summary>
    public Stat Souls = new();

    /// <summary>
    /// The total mana that the player has.
    /// 'Standard' currency.
    /// </summary>
    public Stat Mana = new();

    /// <summary>
    /// The total harvested items that the player has.
    /// This varies depending on the 'level'.
    /// This would represent things like bones, blood, etc.
    /// </summary>
    public Stat Harvested = new();

    /// <summary>
    /// The total victims that the player has murdered.
    /// </summary>
    public Stat Victims = new();

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        // Ensure editor changes 'dirty' the stats.
        Souls.Dirty();
        Mana.Dirty();
        Harvested.Dirty();
        Victims.Dirty();
    }

    public void Load(PersistentModel model)
    {
        var container = model.GetContainer("BANK");
        if (container == null)
        {
            return;
        }

        var persistentSouls = container.EnsureDouble("TOTAL_SOULS");
        var persistentInfluence = container.EnsureDouble("TOTAL_MANA");
        var persistentVictims = container.EnsureDouble("TOTAL_VICTIMS");
        var persistentHarvested = container.EnsureDouble("TOTAL_HARVESTED");

        Souls.Value = persistentSouls.Value;
        Mana.Value = persistentInfluence.Value;
        Victims.Value = persistentVictims.Value;
        Harvested.Value = persistentHarvested.Value;
    }

    public void Save(PersistentModel model)
    {
        var container = model.EnsureContainer("BANK");

        var persistentSouls = container.EnsureDouble("TOTAL_SOULS");
        var persistentInfluence = container.EnsureDouble("TOTAL_MANA");
        var persistentVictims = container.EnsureDouble("TOTAL_VICTIMS");
        var persistentHarvested = container.EnsureDouble("TOTAL_HARVESTED");

        persistentSouls.Value = Souls.Value;
        persistentInfluence.Value = Mana.Value;
        persistentVictims.Value = Victims.Value;
        persistentHarvested.Value = Harvested.Value;
    }
}

[Serializable]
public class Stat
{
    [SerializeField]
    private double value;

    public double Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
            Version++;
        }
    }

    public ulong Version { get; private set; }

    internal void Dirty()
    {
        Version++;
    }
}