using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResolver
{
    void Register(object target, Type type);
    void Unregister(Type type);
    object Resolve(Type type);
}

public class ObjectResolver : IResolver
{
    private readonly Dictionary<Type, object> registry;

    public ObjectResolver()
    {
        registry = new Dictionary<Type, object>();
    }

    public void Register(object target, Type type)
    {
        if (registry.ContainsKey(type))
        {
            throw new InvalidOperationException("Duplicate registration");
        }

        registry.Add(type, target);
    }

    public void Unregister(Type type)
    {
        registry.Remove(type);
    }

    public object Resolve(Type type)
    {
        registry.TryGetValue(type, out object result);
        return result;
    }
}

public static class ResolverExtensions
{
    public static T Resolve<T>(this IResolver resolver)
        where T : class
    {
        return resolver.Resolve(typeof(T)) as T;
    }

    public static void Register<T>(this IResolver resolver, object target)
    {
        resolver.Register(target, typeof(T));
    }

    public static void Unregister<T>(this IResolver resolver)
    {
        resolver.Unregister(typeof(T));
    }
}