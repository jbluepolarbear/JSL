// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace JSL.Utility
{
    /// <summary>
    /// A basic generic thread-unsafe singleton container.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class, requiring a parameterless constructor.</typeparam>
    public class Singleton<T> where T: new ()
    {
        private static T _instance;

        /// <summary>
        /// Gets the single static instance of type <typeparamref name="T"/>, creating it on first access.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }

                return _instance;
            }
        }
    }
}