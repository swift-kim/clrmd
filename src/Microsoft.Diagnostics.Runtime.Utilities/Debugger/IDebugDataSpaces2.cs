﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace Microsoft.Diagnostics.Runtime.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("7a5e852f-96e9-468f-ac1b-0b3addc4a049")]
    public interface IDebugDataSpaces2 : IDebugDataSpaces
    {
        /* IDebugDataSpaces */

        [PreserveSig]
        new int ReadVirtual(
            [In] ulong Offset,
            [Out]
            IntPtr buffer,
            [In] int BufferSize,
            [Out] out int BytesRead);

        [PreserveSig]
        new int WriteVirtual(
            [In] ulong Offset,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        new int SearchVirtual(
            [In] ulong Offset,
            [In] ulong Length,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] pattern,
            [In] uint PatternSize,
            [In] uint PatternGranularity,
            [Out] out ulong MatchOffset);

        [PreserveSig]
        new int ReadVirtualUncached(
            [In] ulong Offset,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        new int WriteVirtualUncached(
            [In] ulong Offset,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        new int ReadPointersVirtual(
            [In] uint Count,
            [In] ulong Offset,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
            ulong[] Ptrs);

        [PreserveSig]
        new int WritePointersVirtual(
            [In] uint Count,
            [In] ulong Offset,
            [In][MarshalAs(UnmanagedType.LPArray)] ulong[] Ptrs);

        [PreserveSig]
        new int ReadPhysical(
            [In] ulong Offset,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        new int WritePhysical(
            [In] ulong Offset,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        new int ReadControl(
            [In] uint Processor,
            [In] ulong Offset,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] buffer,
            [In] int BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        new int WriteControl(
            [In] uint Processor,
            [In] ulong Offset,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] buffer,
            [In] int BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        new int ReadIo(
            [In] INTERFACE_TYPE InterfaceType,
            [In] uint BusNumber,
            [In] uint AddressSpace,
            [In] ulong Offset,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        new int WriteIo(
            [In] INTERFACE_TYPE InterfaceType,
            [In] uint BusNumber,
            [In] uint AddressSpace,
            [In] ulong Offset,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        new int ReadMsr(
            [In] uint Msr,
            [Out] out ulong MsrValue);

        [PreserveSig]
        new int WriteMsr(
            [In] uint Msr,
            [In] ulong MsrValue);

        [PreserveSig]
        new int ReadBusData(
            [In] BUS_DATA_TYPE BusDataType,
            [In] uint BusNumber,
            [In] uint SlotNumber,
            [In] uint Offset,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        new int WriteBusData(
            [In] BUS_DATA_TYPE BusDataType,
            [In] uint BusNumber,
            [In] uint SlotNumber,
            [In] uint Offset,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        new int CheckLowMemory();

        [PreserveSig]
        new int ReadDebuggerData(
            [In] uint Index,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        [PreserveSig]
        new int ReadProcessorSystemData(
            [In] uint Processor,
            [In] DEBUG_DATA Index,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        /* IDebugDataSpaces2 */

        [PreserveSig]
        int VirtualToPhysical(
            [In] ulong Virtual,
            [Out] out ulong Physical);

        [PreserveSig]
        int GetVirtualTranslationPhysicalOffsets(
            [In] ulong Virtual,
            [Out][MarshalAs(UnmanagedType.LPArray)]
            ulong[] Offsets,
            [In] uint OffsetsSize,
            [Out] out uint Levels);

        [PreserveSig]
        int ReadHandleData(
            [In] ulong Handle,
            [In] DEBUG_HANDLE_DATA_TYPE DataType,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        [PreserveSig]
        int FillVirtual(
            [In] ulong Start,
            [In] uint Size,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] buffer,
            [In] uint PatternSize,
            [Out] out uint Filled);

        [PreserveSig]
        int FillPhysical(
            [In] ulong Start,
            [In] uint Size,
            [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] buffer,
            [In] uint PatternSize,
            [Out] out uint Filled);

        [PreserveSig]
        int QueryVirtual(
            [In] ulong Offset,
            [Out] out MEMORY_BASIC_INFORMATION64 Info);
    }
}