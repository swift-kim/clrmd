using Microsoft.Diagnostics.Runtime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PrintStackTrace
{
    class Program
    {
        private static void PrintStackTraces(string target)
        {
            if (target == null)
                target = Process.GetCurrentProcess().Id.ToString();

            using var dataTarget = int.TryParse(target, out var pid) ?
                DataTarget.PassiveAttachToProcess(pid) : DataTarget.LoadCoreDump(target);
            using var runtime = dataTarget.ClrVersions[0].CreateRuntime();

            foreach (var thread in runtime.Threads)
            {
                if (!thread.IsAlive)
                    continue;

                Console.WriteLine("Thread {0:X}:", thread.OSThreadId);
                foreach (ClrStackFrame frame in thread.EnumerateStackTrace())
                {
                    Console.WriteLine("{0,12:X} {1,12:X} {2}", frame.StackPointer, frame.InstructionPointer, frame);
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            var task = new Thread(() => PrintStackTraces(args.ElementAtOrDefault(0)));
            task.Start();
            task.Join();
        }
    }
}
