using System;

namespace JSL.Utility
{
    public class AssertException : Exception
    {
        
    }
    
    public static class Assert
    {
        public static void True(bool value)
        {
#if UNITY_EDITOR
            if (!value)
            {
                throw new AssertException();
            }
#endif
        }

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