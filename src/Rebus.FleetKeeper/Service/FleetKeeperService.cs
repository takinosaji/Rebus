﻿using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Owin.Hosting;

namespace Rebus.FleetKeeper.Service
{
    partial class FleetKeeperService : ServiceBase
    {
        public const string FullFleetKeeperServiceName = "Rebus FleetKeeper";
        public const string FleetKeeperServiceName = "FleetKeeper";

        IDisposable webApp;

        public FleetKeeperService()
        {
            InitializeComponent();
        }

        static void Main(string[] args)
        {
            var service = new FleetKeeperService();

            switch (args.FirstOrDefault())
            {
                case "-h":
                    Console.WriteLine("Rebus.FleetKeeper.exe [-i(nstall)|-u(ninstall)|-c(onsole)]");
                    break;

                case "-i":
                case "-install":
                    ServiceInstaller.Install(args);
                    break;

                case "-u":
                case "-uninstall":
                    ServiceInstaller.Uninstall(args);
                    break;

                case "-c":
                case "-console":
                    RunAsConsole(args);
                    break;

                default:
                    if (Environment.UserInteractive)
                    {
                        RunAsConsole(args);
                        return;
                    }
                    
                    Run(service);
                    break;

            }
        }

        static void RunAsConsole(string[] args)
        {
            Console.WriteLine("Press 'q' or 'ctrl+c' to exit.");

            var service = new FleetKeeperService();

            var thing = new Signals();
            thing.CtrlCPressed += () => ShutDownInteractive(service);
            thing.CtrlBreakPressed += () => ShutDownInteractive(service);

            service.OnStart(args);

            Console.CancelKeyPress += delegate { service.OnStop(); };
            
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "q")
                    break;
            }

            service.OnStop();
        }

        static void ShutDownInteractive(FleetKeeperService fleetKeeperService)
        {
            fleetKeeperService.OnStop();
            Environment.Exit(0);
        }

        protected override void OnStart(string[] args)
        {
            var url = ConfigurationManager.AppSettings["listenUri"];
            webApp = WebApp.Start<Startup>(url);

            Console.WriteLine("FleetKeeper is listening on {0}", url);
        }

        protected override void OnStop()
        {
            var disposable = webApp;

            if (disposable != null)
            {
                webApp = null;

                Console.WriteLine("Shutting down FleetKeeper");
                disposable.Dispose();
            }
        }
    }
}