// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Desktop;

namespace Microsoft.Diagnostics.Runtime.DacInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AppDomainStoreData : IAppDomainStoreData
    {
        public readonly long SharedDomain;
        public readonly long SystemDomain;
        public readonly int AppDomainCount;

        ulong IAppDomainStoreData.SharedDomain => IntPtr.Size == 4 ? (uint)SharedDomain : (ulong)SharedDomain;
        ulong IAppDomainStoreData.SystemDomain => IntPtr.Size == 4 ? (uint)SystemDomain : (ulong)SystemDomain;
        int IAppDomainStoreData.Count => AppDomainCount;
    }
}