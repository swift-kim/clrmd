﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Diagnostics.Runtime.DacInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SegmentData
    {
        public readonly ulong Address;
        public readonly ulong Allocated;
        public readonly ulong Committed;
        public readonly ulong Reserved;
        public readonly ulong Used;
        public readonly ulong Start;
        public readonly ulong Next;
        public readonly ulong Heap;
        public readonly ulong HighAllocMark;
        public readonly IntPtr Flags;
        public readonly ulong BackgroundAllocated;

        internal SegmentData(ref SegmentData data)
        {
            this = data;

            // Sign extension issues
            if (IntPtr.Size == 4)
            {
                FixupPointer(ref Address);
                FixupPointer(ref Allocated);
                FixupPointer(ref Committed);
                FixupPointer(ref Reserved);
                FixupPointer(ref Used);
                FixupPointer(ref Start);
                FixupPointer(ref Next);
                FixupPointer(ref Heap);
                FixupPointer(ref HighAllocMark);
                FixupPointer(ref BackgroundAllocated);
            }
        }

        private static void FixupPointer(ref ulong ptr)
        {
            ptr = (uint)ptr;
        }
    }
}