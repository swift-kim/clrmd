﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Diagnostics.Runtime.Implementation
{
    public interface IRCWData
    {
        ulong Address { get; }
        ulong IUnknown { get; }
        ulong VTablePointer { get; }
        int RefCount { get; }
        ulong ManagedObject { get; }
        bool Disconnected { get; }
        ulong CreatorThread { get; }

        IReadOnlyList<ComInterfaceData> GetInterfaces();
    }
}