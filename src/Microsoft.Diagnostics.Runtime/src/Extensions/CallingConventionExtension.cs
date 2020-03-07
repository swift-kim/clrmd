// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace Microsoft.Diagnostics.Runtime
{
    internal static class CallingConventionExtension
    {
        internal const CallingConvention Pal =
#if TARGET_LINUX_X86
            CallingConvention.Cdecl;
#else
            CallingConvention.StdCall;
#endif
    }
}
