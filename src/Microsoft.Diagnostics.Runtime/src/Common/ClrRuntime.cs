﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Runtime
{
    /// <summary>
    /// Represents a single runtime in a target process or crash dump.  This serves as the primary
    /// entry point for getting diagnostic information.
    /// </summary>
    public abstract class ClrRuntime : IDisposable
    {
        /// <summary>
        /// Used for internal purposes.
        /// </summary>
        public abstract DacLibrary DacLibrary { get; }

        /// <summary>
        /// The ClrInfo of the current runtime.
        /// </summary>
        public abstract ClrInfo ClrInfo { get; }

        /// <summary>
        /// Returns the DataTarget associated with this runtime.
        /// </summary>
        public abstract DataTarget? DataTarget { get; }

        /// <summary>
        /// Returns whether you are allowed to call into the transitive closure of ClrMD objects created from
        /// this runtime on multiple threads.
        /// </summary>
        public abstract bool IsThreadSafe { get; }

        /// <summary>
        /// Enumerates the list of appdomains in the process.
        /// </summary>
        public abstract IReadOnlyList<ClrAppDomain> AppDomains { get; }

        /// <summary>
        /// The System AppDomain for Desktop CLR (null on .Net Core).
        /// </summary>
        public abstract ClrAppDomain? SystemDomain { get; }

        /// <summary>
        /// The Shared AppDomain for Desktop CLR (null on .Net Core).
        /// </summary>
        public abstract ClrAppDomain? SharedDomain { get; }

        public abstract ClrModule BaseClassLibrary { get; }

        /// <summary>
        /// Enumerates all managed threads in the process.  Only threads which have previously run managed
        /// code will be enumerated.
        /// </summary>
        public abstract IReadOnlyList<ClrThread> Threads { get; }

        /// <summary>
        /// Returns a ClrMethod by its internal runtime handle (on desktop CLR this is a MethodDesc).
        /// </summary>
        /// <param name="methodHandle">The method handle (MethodDesc) to look up.</param>
        /// <returns>The ClrMethod for the given method handle, or null if no method was found.</returns>
        public abstract ClrMethod? GetMethodByHandle(ulong methodHandle);

        /// <summary>
        /// Gets the ClrType corresponding to the given MethodTable.
        /// </summary>
        /// <param name="methodTable">The ClrType.MethodTable for the requested type.</param>
        /// <param name="componentMethodTable">The ClrType's component MethodTable for the requested type.</param>
        /// <returns>A ClrType object, or null if no such type exists.</returns>
        public abstract ClrType? GetTypeByMethodTable(ulong methodTable);

        /// <summary>
        /// Enumerates a list of GC handles currently in the process.  Note that this list may be incomplete
        /// depending on the state of the process when we attempt to walk the handle table.
        /// </summary>
        /// <returns>The list of GC handles in the process, NULL on catastrophic error.</returns>
        public abstract IEnumerable<ClrHandle> EnumerateHandles();

        /// <summary>
        /// Gets the GC heap of the process.
        /// </summary>
        public abstract ClrHeap Heap { get; }

        /// <summary>
        /// Attempts to get a ClrMethod for the given instruction pointer.  This will return NULL if the
        /// given instruction pointer is not within any managed method.
        /// </summary>
        public abstract ClrMethod? GetMethodByInstructionPointer(ulong ip);

        /// <summary>
        /// Enumerate all managed modules in the runtime.
        /// </summary>
        public abstract IEnumerable<ClrModule> EnumerateModules();

        /// <summary>
        /// Flushes the dac cache.  This function MUST be called any time you expect to call the same function
        /// but expect different results.  For example, after walking the heap, you need to call Flush before
        /// attempting to walk the heap again.  After calling this function, you must discard ALL ClrMD objects
        /// you have cached other than DataTarget and ClrRuntime and re-request the objects and data you need.
        /// (E.G. if you want to use the ClrHeap object after calling flush, you must call ClrRuntime.GetHeap
        /// again after Flush to get a new instance.)
        /// </summary>
        public abstract void FlushCachedData();

        /// <summary>
        /// Gets the name of a JIT helper function.
        /// </summary>
        /// <param name="address">Address of a possible JIT helper function.</param>
        /// <returns>The name of the JIT helper function or null if <paramref name="address"/> isn't a JIT helper function.</returns>
        public abstract string? GetJitHelperFunctionName(ulong address);

        /// <summary>
        /// Cleans up all resources and releases them.  You may not use this ClrRuntime or any object it transitively
        /// created after calling this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called when disposing ClrRuntime.
        /// </summary>
        /// <param name="disposing">Whether Dispose() was called or not.</param>
        protected abstract void Dispose(bool disposing);
    }
}