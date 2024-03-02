using Game.Framework;
using System;
using UnityEngine;

public class Bank : GameService<Bank>, IPersistent
{
    /// <summary>
    /// The total souls that the player has.
    /// 'Premium' currency.
    /// </summary>
    public Stat Souls;

    /// <summary>
    /// The total influence that the player has.
    /// 'Standard' currency.
    /// </summary>
    public Stat Influence;

    /// <summary>
    /// The total harvested items that the player has.
    /// This varies depending on the 'level'.
    /// This would represent things like bones, blood, etc.
    /// </summary>
    public Stat Harvested;

    /// <summary>
    /// The total victims that the player has.
    /// </summary>
    public Stat Victims;

    private void OnValidate()
    {
        // Ensure editor changes 'dirty' the stats.
        Souls.Dirty();
        Influence.Dirty();
        Harvested.Dirty();
        Victims.Dirty();
    }

    public void Load(PersistenceModel model)
    {
        var persistentSouls = model.EnsureDouble("TOTAL_SOULS");
        var persistentInfluence = model.EnsureDouble("TOTAL_INFLUENCE");
        var persistentHarvested = model.EnsureDouble("TOTAL_HARVESTED");
        var persistentVictims = model.EnsureDouble("TOTAL_VICTIMS");

        Souls.Value = persistentSouls.Value;
        Influence.Value = persistentInfluence.Value;
        Harvested.Value = persistentHarvested.Value;
        Victims.Value = persistentVictims.Value;
    }

    public void Save(PersistenceModel model)
    {
        var persistentSouls = model.EnsureDouble("TOTAL_SOULS");
        var persistentInfluence = model.EnsureDouble("TOTAL_INFLUENCE");
        var persistentHarvested = model.EnsureDouble("TOTAL_HARVESTED");
        var persistentVictims = model.EnsureDouble("TOTAL_VICTIMS");

        persistentSouls.Value = Souls.Value;
        persistentInfluence.Value = Influence.Value;
        persistentHarvested.Value = Harvested.Value;
        persistentVictims.Value = Harvested.Value;
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
