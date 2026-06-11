// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Numerics;

namespace JSL.Buffers
{
    /// <summary>
    /// Internal unsafe memory utility for copy operations.
    /// </summary>
    internal static unsafe class UnsafeMemory
    {
        /// <summary>
        /// Unsafely copies a block of uint memory.
        /// </summary>
        public static void Copy(uint* src, uint* dst, int length)
        {
            for (var i = 0; i < length; ++i)
            {
                dst[i] = src[i];
            }
        }
        
        /// <summary>
        /// Unsafely copies a block of byte memory.
        /// </summary>
        public static void Copy(byte* src, byte* dst, int length)
        {
            for (var i = 0; i < length; ++i)
            {
                dst[i] = src[i];
            }
        }
    }

    /// <summary>
    /// Collection of high-performance memory copy, casting, endian conversion, and math operations.
    /// </summary>
    public static class Memory
    {
        /// <summary>
        /// Gets the raw integer value of the float
        /// This is not a cast, this interprets the float as an int. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int FloatToInt(float value)
        {
            unsafe
            {
                var ptr = &value;
                return *(int*) ptr;
            }
        }

        public static float IntToFloat(int value)
        {
            unsafe
            {
                var ptr = &value;
                return *(float*) ptr;
            }
        }
        
        public static long DoubleToLong(double value)
        {
            unsafe
            {
                var ptr = &value;
                return *(long*) ptr;
            }
        }

        public static double LongToDouble(long value)
        {
            unsafe
            {
                var ptr = &value;
                return *(double*) ptr;
            }
        }

        public static int BitsRequired(int min, int max)
        {
            int val = max - min;
            if (val <= 0)
            {
                return 0;
            }
            int bits = 0;
            while (val > 0)
            {
                bits++;
                val >>= 1;
            }
            return bits;
        }
        
        public static ushort NetworkToHost(ushort value)
        {
            return (ushort) IPAddress.NetworkToHostOrder((ushort) value);
        }
        
        public static uint NetworkToHost(uint value)
        {
            return (uint) IPAddress.NetworkToHostOrder((int) value);
        }
        
        public static ulong NetworkToHost(ulong value)
        {
            return (ulong) IPAddress.NetworkToHostOrder((long) value);
        }
        
        public static ushort HostToNetwork(ushort value)
        {
            return (ushort) IPAddress.HostToNetworkOrder((ushort) value);
        }
        
        public static uint HostToNetwork(uint value)
        {
            return (uint) IPAddress.HostToNetworkOrder((int) value);
        }
        
        public static ulong HostToNetwork(ulong value)
        {
            return (ulong) IPAddress.HostToNetworkOrder((long) value);
        }

        /// <summary>
        /// Copy from byte array to uint array
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="srcOffset"></param>
        /// <param name="dstOffset"></param>
        /// <param name="length"></param>
        public static void Copy(byte[] src, uint[] dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (byte* srcPtr = src)
                {
                    fixed (uint* dstPtr = dst)
                    {
                        UnsafeMemory.Copy(srcPtr + srcOffset, (byte*) (dstPtr + dstOffset), length);
                    }
                }
            }
        }
        
        public static void Copy(ReadOnlySpan<byte> src, Span<uint> dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (byte* srcPtr = src)
                {
                    fixed (uint* dstPtr = dst)
                    {
                        UnsafeMemory.Copy(srcPtr + srcOffset, (byte*) (dstPtr + dstOffset), length);
                    }
                }
            }
        }
        
        public static void Copy(uint[] src, uint[] dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (uint* srcPtr = src)
                {
                    fixed (uint* dstPtr = dst)
                    {
                        UnsafeMemory.Copy((byte*) (srcPtr + srcOffset), (byte*) (dstPtr + dstOffset), length);
                    }
                }
            }
        }
        
        public static void Copy(ReadOnlySpan<uint> src, Span<uint> dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (uint* srcPtr = src)
                {
                    fixed (uint* dstPtr = dst)
                    {
                        UnsafeMemory.Copy((byte*) (srcPtr + srcOffset), (byte*) (dstPtr + dstOffset), length);
                    }
                }
            }
        }
        
        public static void Copy(uint[] src, byte[] dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (uint* srcPtr = src)
                {
                    fixed (byte* dstPtr = dst)
                    {
                        UnsafeMemory.Copy( (byte*) (srcPtr + srcOffset),  dstPtr + dstOffset, length);
                    }
                }
            }
        }
        
        public static void Copy(ReadOnlySpan<uint> src, Span<byte> dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (uint* srcPtr = src)
                {
                    fixed (byte* dstPtr = dst)
                    {
                        UnsafeMemory.Copy( (byte*) (srcPtr + srcOffset),  dstPtr + dstOffset, length);
                    }
                }
            }
        }
        
        public static void Copy(byte[] src, byte[] dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (byte* srcPtr = src)
                {
                    fixed (byte* dstPtr = dst)
                    {
                        UnsafeMemory.Copy( srcPtr + srcOffset,  dstPtr + dstOffset, length);
                    }
                }
            }
        }
        
        public static void Copy(ReadOnlySpan<byte> src, Span<byte> dst, int srcOffset, int dstOffset, int length)
        {
            unsafe
            {
                fixed (byte* srcPtr = src)
                {
                    fixed (byte* dstPtr = dst)
                    {
                        UnsafeMemory.Copy( srcPtr + srcOffset,  dstPtr + dstOffset, length);
                    }
                }
            }
        }
    }
}