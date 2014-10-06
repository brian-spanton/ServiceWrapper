using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Foobalator;

namespace ServiceWrapper
{
    // BrianSp: This class manages the service when run in that mode.
    public class Service : ServiceBase
    {
        private static Service s_Service;

        private Worker worker = new Worker();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            s_Service = new Service();
            s_Service.Call(args);
        }

        void Call(string[] args)
        {
            SystemState.Settings = new ConfigFileSettings();

            try
            {
                // BrianSp: This app can be run as a command line utility if you pass any argument to it.  Otherwise it assumes
                // it is executing in the context of an installed windows service.

                if (args.Length == 0)
                {
                    ServiceBase.Run(new ServiceBase[] { new Service() });
                }
                else if (args.Length == 1)
                {
                    if (args[0] == "/console" || args[0] == "/c")
                        worker.Run();
                    else
                        Help();
                }
                else
                {
                    Help();
                }
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
        }

        private void Help()
        {
            Log.ShowLine(Worker.Name + " usage:");
            Log.ShowLine("    /console or /c - Run service as a console app");
            Log.ShowLine("    (anything else) - Display this help text");
        }

        public Service()
        {
            ServiceName = Worker.Name;
        }

        protected override void OnStart(string[] args)
        {
            // BrianSp: Kick off a new thread because the service manager owns this one.  This thread
            // runs the main application entry point.
            Thread thread = new Thread(() => { ServiceRun(); });
            thread.Start();
        }

        private void ServiceRun()
        {
            try
            {
                worker.Run();
            }
            catch
            {
                Stop();
            }
        }

        protected override void OnStop()
        {
            // BrianSp: If the service control manager sends a stop message, set the event to let the app thread
            // know it's time to shutdown.
            worker.End();
        }
    }
}
