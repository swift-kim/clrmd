// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Desktop;

namespace Microsoft.Diagnostics.Runtime.DacInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AppDomainData : IAppDomainData
    {
        public readonly long Address;
        public readonly long SecurityDescriptor;
        public readonly long LowFrequencyHeap;
        public readonly long HighFrequencyHeap;
        public readonly long StubHeap;
        public readonly long DomainLocalBlock;
        public readonly long DomainLocalModules;
        public readonly int Id;
        public readonly int AssemblyCount;
        public readonly int FailedAssemblyCount;
        public readonly int Stage;

        int IAppDomainData.Id => Id;
        ulong IAppDomainData.Address => IntPtr.Size == 4 ? (uint)Address : (ulong)Address;
        ulong IAppDomainData.LowFrequencyHeap => IntPtr.Size == 4 ? (uint)LowFrequencyHeap : (ulong)LowFrequencyHeap;
        ulong IAppDomainData.HighFrequencyHeap => IntPtr.Size == 4 ? (uint)HighFrequencyHeap : (ulong)HighFrequencyHeap;
        ulong IAppDomainData.StubHeap => IntPtr.Size == 4 ? (uint)StubHeap : (ulong)StubHeap;
        int IAppDomainData.AssemblyCount => AssemblyCount;
    }
}