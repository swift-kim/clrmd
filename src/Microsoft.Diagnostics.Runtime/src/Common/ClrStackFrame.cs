﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Diagnostics.Runtime
{
    /// <summary>
    /// A frame in a managed stack trace.  Note you can call ToString on an instance of this object to get the
    /// function name (or clr!Frame name) similar to SOS's !clrstack output.
    /// </summary>
    public abstract class ClrStackFrame
    {
        /// <summary>
        /// Gets this stack frame context.
        /// </summary>
        public abstract ReadOnlySpan<byte> Context { get; }

        /// <summary>
        /// The instruction pointer of this frame.
        /// </summary>
        public abstract ulong InstructionPointer { get; }

        /// <summary>
        /// The stack pointer of this frame.
        /// </summary>
        public abstract ulong StackPointer { get; }

        /// <summary>
        /// The type of frame (managed or internal).
        /// </summary>
        public abstract ClrStackFrameKind Kind { get; }

        /// <summary>
        /// Returns the ClrMethod which corresponds to the current stack frame.  This may be null if the
        /// current frame is actually a CLR "Internal Frame" representing a marker on the stack, and that
        /// stack marker does not have a managed method associated with it.
        /// </summary>
        public abstract ClrMethod? Method { get; }

        /// <summary>
        /// The helper method frame name if Kind == Runtime, null otherwise.
        /// </summary>
        public abstract string? FrameName { get; }
    }
}