﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Diagnostics.Runtime.Linux;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Microsoft.Diagnostics.Runtime
{
    internal class CoreDumpReader : IDataReader
    {
        private readonly string _source;
        private readonly Stream _stream;
        private readonly ElfCoreFile _core;
        private Dictionary<uint, IElfPRStatus>? _threads;
        private List<ModuleInfo>? _modules;

        public CoreDumpReader(string filename)
        {
            _source = filename;
            _stream = File.OpenRead(filename);
            _core = new ElfCoreFile(_stream);

            ElfMachine architecture = _core.ElfFile.Header.Architecture;
            switch (architecture)
            {
                case ElfMachine.EM_X86_64:
                    PointerSize = 8;
                    Architecture = Architecture.Amd64;
                    break;

                case ElfMachine.EM_386:
                    PointerSize = 4;
                    Architecture = Architecture.X86;
                    break;

                case ElfMachine.EM_AARCH64:
                    PointerSize = 8;
                    Architecture = Architecture.Arm64;
                    break;

                case ElfMachine.EM_ARM:
                    PointerSize = 4;
                    Architecture = Architecture.Arm;
                    break;

                default:
                    throw new NotImplementedException($"Support for {architecture} not yet implemented.");
            }
        }

        public bool IsThreadSafe => false;
        public bool IsFullMemoryAvailable => true; // TODO

        public void Dispose()
        {
            _stream.Dispose();
        }

        public uint ProcessId
        {
            get
            {
                foreach (IElfPRStatus status in _core.EnumeratePRStatus())
                    return status.ProcessId;

                return uint.MaxValue;
            }
        }

        public IEnumerable<uint> EnumerateAllThreads()
        {
            InitThreads();
            return _threads!.Keys;
        }

        public IList<ModuleInfo> EnumerateModules()
        {
            if (_modules is null)
            {
                // Need to filter out non-modules like the interpreter (named something
                // like "ld-2.23") and anything that starts with /dev/ because their
                // memory range overlaps with actual modules.
                ulong interpreter = _core.GetAuxvValue(ElfAuxvType.Base);

                _modules = new List<ModuleInfo>(_core.LoadedImages.Count);
                foreach (ElfLoadedImage image in _core.LoadedImages)
                    if ((ulong)image.BaseAddress != interpreter && !image.Path.StartsWith("/dev"))
                        _modules.Add(CreateModuleInfo(image));
            }

            return _modules;
        }

        private ModuleInfo CreateModuleInfo(ElfLoadedImage image)
        {
            ElfFile? file = image.Open();

            uint filesize = (uint)image.Size;
            uint timestamp = 0;

            if (file is null)
            {
                PEImage pe = image.OpenAsPEImage();
                filesize = (uint)pe.IndexFileSize;
                timestamp = (uint)pe.IndexTimeStamp;
            }

            return new ModuleInfo(this, (ulong)image.BaseAddress, filesize, timestamp, image.Path, file?.BuildId);
        }

        public void FlushCachedData()
        {
            _threads = null;
            _modules = null;
        }

        public Architecture Architecture { get; }

        public int PointerSize { get; }

        public bool GetThreadContext(uint threadID, uint contextFlags, Span<byte> context)
        {
            InitThreads();

            if (_threads!.TryGetValue(threadID, out IElfPRStatus status))
                return status.CopyContext(contextFlags, context);

            return false;
        }

        public unsafe void GetVersionInfo(ulong baseAddress, out VersionInfo version)
        {
            ElfLoadedImage image = _core.LoadedImages.First(image => (ulong)image.BaseAddress == baseAddress);
            ElfFile? file = image.Open();
            if (file is null)
                version = default;
            else
                LinuxFunctions.GetVersionInfo(this, baseAddress, file, out version);
        }

        public bool ReadMemory(ulong address, Span<byte> buffer, out int bytesRead)
        {
            bytesRead = _core.ReadMemory((long)address, buffer);
            return bytesRead > 0;
        }

        public ulong ReadPointerUnsafe(ulong addr)
        {
            Span<byte> buffer = stackalloc byte[IntPtr.Size];

            if (_core.ReadMemory((long)addr, buffer) == IntPtr.Size)
                return buffer.AsPointer();

            return 0;
        }

        public unsafe bool Read<T>(ulong addr, out T value) where T : unmanaged
        {
            Span<byte> buffer = stackalloc byte[sizeof(T)];
            if (!ReadMemory(addr, buffer, out _))
            {
                value = Unsafe.As<byte, T>(ref buffer[0]);
                return true;
            }

            value = default;
            return false;
        }

        public T ReadUnsafe<T>(ulong addr) where T : unmanaged
        {
            Read(addr, out T value);
            return value;
        }

        public bool ReadPointer(ulong address, out ulong value)
        {
            Span<byte> buffer = stackalloc byte[IntPtr.Size];
            if (!ReadMemory(address, buffer, out _))
            {
                value = buffer.AsPointer();
                return true;
            }

            value = 0;
            return false;
        }

        public bool VirtualQuery(ulong address, out VirtualQueryData vq)
        {
            long addr = (long)address;
            foreach (ElfProgramHeader item in _core.ElfFile.ProgramHeaders)
            {
                long start = item.VirtualAddress;
                long end = start + item.VirtualSize;

                if (start <= addr && addr < end)
                {
                    vq = new VirtualQueryData((ulong)start, (ulong)item.VirtualSize);
                    return true;
                }
            }

            vq = new VirtualQueryData();
            return false;
        }

        private void InitThreads()
        {
            if (_threads != null)
                return;

            _threads = new Dictionary<uint, IElfPRStatus>();
            foreach (IElfPRStatus status in _core.EnumeratePRStatus())
                _threads.Add(status.ThreadId, status);
        }
    }
}