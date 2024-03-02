using System;
using UnityEngine;

[DefaultExecutionOrder(-5000)]
public class Resolver : MonoBehaviour, IResolver
{
    [SerializeField]
    private Resolver parent;

    private IResolver resolver;

    protected virtual void Awake()
    {
        resolver = InitializeResolver();
    }

    public void SetParent(Resolver resolver)
    {
        parent = resolver;
    }

    protected virtual IResolver InitializeResolver()
    {
        return new ObjectResolver();
    }

    public void Register(object target, Type type)
    {
        resolver.Register(target, type);
    }

    public void Unregister(Type type)
    {
        resolver.Unregister(type);
    }

    public object Resolve(Type type)
    {
        var result = resolver.Resolve(type);

        if(result == null)
        {
            return parent.Resolve(type);
        }
        else
        {
            return result;
        }
    }
}
