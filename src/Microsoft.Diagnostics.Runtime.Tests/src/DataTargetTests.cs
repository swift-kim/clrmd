﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Runtime.DacInterface;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Microsoft.Diagnostics.Runtime.Tests
{
    public class DataTargetTests : IDisposable
    {
        private readonly NoFailContext _context = new NoFailContext();

        public void Dispose() => _context.Dispose();

        [Fact]
        public void PassiveAttachToProcess()
        {
            using Process process = CreateProcess();

            try
            {
                using DataTarget dataTarget = DataTarget.PassiveAttachToProcess(process.Id);
                ProcessThread mainThread = GetMainThread(process, dataTarget);

                Assert.Equal(ThreadState.Running, mainThread.ThreadState);
            }
            finally
            {
                process.Kill();
            }
        }

        [Fact]
        public void SuspendAndAttachToProcess()
        {
            using Process process = CreateProcess();

            try
            {
                using DataTarget dataTarget = DataTarget.SuspendAndAttachToProcess(process.Id);
                ProcessThread mainThread = GetMainThread(process, dataTarget);

                Assert.Equal(ThreadState.Wait, mainThread.ThreadState);
            }
            finally
            {
                process.Kill();
            }
        }

        private static Process CreateProcess()
        {
            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = TestTargets.Spin.Executable,
                Arguments = "_",
                RedirectStandardOutput = true,
            });

            _ = process.StandardOutput.ReadLine();
            return process;
        }

        private static ProcessThread GetMainThread(Process process, DataTarget dataTarget)
        {
            using ClrRuntime runtime = dataTarget.ClrVersions.Single().CreateRuntime();
            uint mainThreadId = runtime.GetMainThread().OSThreadId;
            return process.Threads.Cast<ProcessThread>().Single(thread => thread.Id == mainThreadId);
        }

        [Fact]
        public void EnsureFinalReleaseOfInterfaces()
        {
            using DataTarget dt = TestTargets.Types.LoadFullDump();

            RefCountedFreeLibrary library;
            SOSDac sosDac;

            using (ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime())
            {
                library = runtime.DacLibrary.OwningLibrary;
                sosDac = runtime.DacLibrary.SOSDacInterface;

                // Keep library alive
                library.AddRef();
            }

            sosDac.Dispose();
            Assert.Equal(0, library.Release());
        }

        [LinuxFact]
        public void CreateSnapshotAndAttach_ThrowsPlatformNotSupportedException()
        {
            _ = Assert.Throws<PlatformNotSupportedException>(() => DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id));
        }

        [WindowsFact]
        public void LoadCoreDump_ThrowsPlatformNotSupportedException()
        {
            _ = Assert.Throws<PlatformNotSupportedException>(() => DataTarget.LoadCoreDump(TestTargets.Types.BuildDumpName(GCMode.Workstation, true)));
        }

        [LinuxFact]
        public void LoadCrashDump_ThrowsPlatformNotSupportedException()
        {
            _ = Assert.Throws<PlatformNotSupportedException>(() => DataTarget.LoadCrashDump(TestTargets.Types.BuildDumpName(GCMode.Workstation, true)));
        }
    }
}
