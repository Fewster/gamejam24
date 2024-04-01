using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    [DefaultExecutionOrder(-500)]
    public class GameBehaviour : MonoBehaviour
    {
        private bool setup;

        public IResolver Resolver { get; private set; }

        private void Start()
        {
            Initialize ();
        }

        private void OnDestroy()
        {
            if (setup)
            {
                setup = false;
                OnCleanup();
            }
        }

        protected virtual IResolver AcquireResolver()
        {
            return GetComponentInParent<IResolver>();
        }

        private void Initialize()
        {
            Resolver = AcquireResolver();

            if (Resolver == null)
            {
                Debug.LogError("Failed to find a Resolver. The object will be disabled.", this);
                enabled = false;
                return;
            }

            setup = true;

            OnSetup();
        }

        protected virtual void OnSetup() { }
        protected virtual void OnCleanup() { }
    }
}
