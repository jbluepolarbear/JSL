// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;
using JSL.Pools;

namespace JSL.NetTypes
{
    /// <summary>
    /// Base class for reference-counted, recyclable resources pooled within the network architecture.
    /// </summary>
    public abstract class NetRecyclable: INetRecyclable
    {
        /// <summary>
        /// Gets the current reference count of the resource.
        /// </summary>
        public int RefCount { get; private set; } = 0;

        /// <summary>
        /// Gets a value indicating whether this instance is currently active and acquired.
        /// </summary>
        public bool InstanceActive { get; private set; } = false;

        /// <summary>
        /// Callback triggered when reference count drops to 0, notifying the parent pool to recycle the object.
        /// </summary>
        internal Action<NetRecyclable> OnRelease;

        /// <summary>
        /// Increments the reference count. Triggers <see cref="AcquireImpl"/> if the resource was inactive.
        /// </summary>
        /// <returns>An IDisposable token representing reference ownership.</returns>
        public IDisposable Acquire()
        {
            if (RefCount++ == 0)
            {
                AcquireImpl();
            }

            return this;
        }

        /// <summary>
        /// Decrements the reference count. Triggers <see cref="ReleaseImpl"/> and invokes <see cref="OnRelease"/> when count reaches 0.
        /// </summary>
        private void Release()
        {
            if (RefCount == 0)
            {
                return;
            }

            if (--RefCount > 0)
            {
                return;
            }

            ReleaseImpl();
            OnRelease?.Invoke(this);
        }

        /// <summary>
        /// Virtual callback called when the resource reference count rises from 0.
        /// </summary>
        protected virtual void AcquireImpl()
        {
            InstanceActive = true;
        }

        /// <summary>
        /// Virtual callback called when the resource reference count falls to 0.
        /// </summary>
        protected virtual void ReleaseImpl()
        {
            InstanceActive = false;
        }

        /// <summary>
        /// Reference to the global <see cref="Pools.MemoryManager"/> singleton.
        /// </summary>
        protected MemoryManager MemoryManager => MemoryManager.Instance;

        /// <summary>
        /// Disposes the resource, releasing the active reference.
        /// </summary>
        public void Dispose()
        {
            Release();
        }
    }
}