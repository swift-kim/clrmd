﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Runtime.Implementation
{
    public interface ITypeFactory : IDisposable
    {
        bool IsThreadSafe { get; }

        ClrRuntime GetOrCreateRuntime();
        ClrHeap GetOrCreateHeap();
        ClrModule GetOrCreateModule(ClrAppDomain domain, ulong address);
        bool CreateMethodsForType(ClrType type, out IReadOnlyList<ClrMethod> methods);
        bool CreateFieldsForType(ClrType type, out IReadOnlyList<ClrInstanceField> fields, out IReadOnlyList<ClrStaticField> staticFields);
        ComCallWrapper? CreateCCWForObject(ulong obj);
        RuntimeCallableWrapper? CreateRCWForObject(ulong obj);
        ClrType CreateSystemType(ClrHeap heap, ulong mt, string kind);
        ClrType? GetOrCreateType(ClrHeap heap, ulong mt, ulong obj);
        ClrType? GetOrCreateType(ulong mt, ulong obj);
        ClrType GetOrCreateBasicType(ClrElementType basicType);
        ClrType? GetOrCreateArrayType(ClrType inner, int ranks);
        ClrType? GetOrCreateTypeFromToken(ClrModule module, uint token);
        ClrType? GetOrCreatePointerType(ClrType innerType, int depth);
        ClrMethod? CreateMethodFromHandle(ulong methodHandle);
    }
}