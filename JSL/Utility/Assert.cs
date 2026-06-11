// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace JSL.Utility
{
    /// <summary>
    /// Exception thrown when a JSL library assertion fails.
    /// </summary>
    public class AssertException : Exception
    {
        
    }
    
    /// <summary>
    /// Assertion helper class designed to perform validation checks.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Asserts that a boolean condition is true. Throws an <see cref="AssertException"/> if it is false.
        /// Only evaluated and executed in Editor builds (when <c>UNITY_EDITOR</c> is defined).
        /// </summary>
        /// <param name="value">The condition to evaluate.</param>
        [Conditional("UNITY_EDITOR")]
        public static void True(bool value)
        {
#if UNITY_EDITOR
            if (!value)
            {
                throw new AssertException();
            }
#endif
        }

        /// <summary>
        /// Asserts that the specified object is not null. Throws an <see cref="AssertException"/> if it is null.
        /// Only evaluated and executed in Editor builds (when <c>UNITY_EDITOR</c> is defined).
        /// </summary>
        /// <param name="obj">The object reference to check.</param>
        [Conditional("UNITY_EDITOR")]
        public static void NotNull(object obj)
        {
#if UNITY_EDITOR
            if (obj == null)
            {
                throw new AssertException();
            }
#endif
        }
    }
}