// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a recyclable network resource that tracks its reference count.
    /// </summary>
    public interface INetRecyclable: IDisposable
    {
        /// <summary>
        /// Gets the current reference count of this recyclable resource.
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// Acquires a reference to this resource, incrementing its reference count.
        /// </summary>
        /// <returns>An IDisposable token representing reference ownership.</returns>
        IDisposable Acquire();
    }
}