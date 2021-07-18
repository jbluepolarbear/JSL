using System;
using System.Collections.Generic;

namespace JSL.Utility
{
    public static class MathHelpers
    {
        /*
         * const __powerOf2Cache: Record<number, number> = {};

export function powerOf2(power: number): number {
  if (power === 1) {
    return 2;
  }

  if (__powerOf2Cache.hasOwnProperty(power)) {
    return __powerOf2Cache[power];
  }

  const po2 = powerOf2(power - 1) * 2;
  __powerOf2Cache[power] = po2;
  return po2;
}
         */
        private static List<int> _powerOf2Cache = new List<int>
        {
            1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096
        };
        public static int PowerOf2(int power)
        {
            if (_powerOf2Cache.Count <= power)
            {
                var value = PowerOf2(power - 1) * 2;
                _powerOf2Cache.Add(value);
            }

            return _powerOf2Cache[power];
        }
    }
}