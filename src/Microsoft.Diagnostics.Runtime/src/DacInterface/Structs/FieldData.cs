﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace Microsoft.Diagnostics.Runtime.DacInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FieldData
    {
        public readonly uint ElementType; // CorElementType
        public readonly uint SigType; // CorElementType
        public readonly ulong TypeMethodTable; // NULL if Type is not loaded
        public readonly ulong TypeModule;
        public readonly uint TypeToken;
        public readonly uint FieldToken;
        public readonly ulong MTOfEnclosingClass;
        public readonly uint Offset;
        public readonly uint IsThreadLocal;
        public readonly uint IsContextLocal;
        public readonly uint IsStatic;
        public readonly ulong NextField;
    }
}