using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Foobalator;

namespace ServiceWrapper
{
    public class Worker
    {
        public static readonly string Name = System.IO.Path.GetFileName(typeof(Worker).Assembly.Location);

        private Foobalator.Process process;
        private bool running;

        // BrianSp: This is the main entry point for the main thread.  Spins up worker threads and waits for signal to stop.
        public void Run()
        {
            Log.WriteLine(Name + " started");

            try
            {
                running = true;

                while (running)
                {
                    string command = SystemState.Settings.GetValue("Worker.Run.Command");
                    string args = SystemState.Settings.GetValue("Worker.Run.Args");
                    string workingDir = SystemState.Settings.GetValue("Worker.Run.WorkingDir");

                    DataReceivedEventHandler outputHandler = (sender, output) =>
                        {
                            if (output.Data != null)
                            {
                                Log.WriteLine(output.Data);
                            }
                        };

                    Log.WriteLine(command + " started");

                    process = new Foobalator.Process(command, args, workingDir, false, System.Diagnostics.ProcessPriorityClass.Normal, outputHandler);
                    process.WaitForExit(null);

                    Log.WriteLine(string.Format(command + " stopped and returned {0}", process.ExitCode));
                }
            }
            finally
            {
                if (process != null)
                {
                    try
                    {
                        process.Stop();
                    }
                    catch
                    {
                    }
                }

                Log.WriteLine(Name + " stopped");
            }
        }

        public void End()
        {
            Log.WriteLine(Name + " stopping");

            try
            {
                process.Input("stop\n");
            }
            catch
            {
            }

            running = false;
        }
    }
}