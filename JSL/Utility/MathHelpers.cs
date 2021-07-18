using System;

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
        public static int PowerOf2(int power)
        {
            return (int) Math.Pow(2, power);
        }
    }
}