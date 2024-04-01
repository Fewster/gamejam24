using Game.Framework;

public abstract class Structure : GameBehaviour
{
    public string Type;

    public virtual void Load(PersistentContainer container) { }
    public virtual void Save(PersistentContainer container) { }
}
