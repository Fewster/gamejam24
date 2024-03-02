using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    [DefaultExecutionOrder(-1000)]
    public abstract class GameService : MonoBehaviour
    {
        public IResolver Resolver { get; private set; }

        protected virtual void Awake()
        {
            Resolver = AcquireResolver();

            if (Resolver == null)
            {
                Debug.LogError("Failed to find a Resolver.", this);
                return;
            }

            Initialize();
        }

        protected virtual void Start()
        {
            OnSetup();
        }

        protected virtual void OnDestroy()
        {
            OnCleanup();
        }

        protected virtual IResolver AcquireResolver()
        {
            return GetComponentInParent<IResolver>();
        }

        internal virtual void Cleanup()
        {
            OnCleanup();
        }

        internal virtual void Initialize()
        {
            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnSetup() { }
        protected virtual void OnCleanup() { }
    }

    public abstract class GameService<T> : GameService
    {
        internal override void Initialize()
        {
            Resolver.Register<T>(this);

            OnInitialize();
        }

        internal override void Cleanup()
        {
            Resolver.Unregister<T>();

            OnCleanup();
        }
    }

    public abstract class GameService<T1, T2> : GameService
    {
        internal override void Initialize()
        {
            Resolver.Register<T1>(this);
            Resolver.Register<T2>(this);

            OnInitialize();
        }

        internal override void Cleanup()
        {
            Resolver.Unregister<T1>();
            Resolver.Unregister<T2>();

            OnCleanup();
        }
    }
}