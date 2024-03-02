using System.Threading.Tasks;
using UnityEngine;

public abstract class PersistenceProvider : ScriptableObject
{
    public abstract Task Save(byte[] model);
    public abstract Task<byte[]> Load();
}
