﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Desktop;

namespace Microsoft.Diagnostics.Runtime.DacInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AssemblyData : IAssemblyData
    {
        public readonly long Address;
        public readonly long ClassLoader;
        public readonly long ParentDomain;
        public readonly long AppDomain;
        public readonly long AssemblySecurityDescriptor;
        public readonly int Dynamic;
        public readonly int ModuleCount;
        public readonly uint LoadContext;
        public readonly int IsDomainNeutral;
        public readonly uint LocationFlags;

        ulong IAssemblyData.Address => IntPtr.Size == 4 ? (uint)Address : (ulong)Address;
        ulong IAssemblyData.ParentDomain => IntPtr.Size == 4 ? (uint)ParentDomain : (ulong)ParentDomain;
        ulong IAssemblyData.AppDomain => IntPtr.Size == 4 ? (uint)AppDomain : (ulong)AppDomain;
        bool IAssemblyData.IsDynamic => Dynamic != 0;
        bool IAssemblyData.IsDomainNeutral => IsDomainNeutral != 0;
        int IAssemblyData.ModuleCount => ModuleCount;
    }
}