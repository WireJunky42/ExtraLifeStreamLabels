﻿using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace WireJunky.ServiceFramework
{
    public class BasicServiceStarter
    {
        public static void Run<T>(string serviceName) where T : IService, new()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (EventLog.SourceExists(serviceName))
                {
                    EventLog.WriteEntry(serviceName,
                        "Fatal Exception : " + Environment.NewLine +
                        e.ExceptionObject, EventLogEntryType.Error);
                }
            };

            if (Environment.UserInteractive)
            {
                var cmd = (Environment.GetCommandLineArgs().Skip(1).FirstOrDefault() ?? "").ToLower();

                switch (cmd)
                {
                    case "i":
                    case "install":
                        Console.WriteLine("Installing {0}", serviceName);
                        BasicServiceInstaller.Install(serviceName);
                        break;
                    case "u":
                    case "uninstall":
                        Console.WriteLine("Uninstalling {0}", serviceName);
                        BasicServiceInstaller.Uninstall(serviceName);
                        break;
                    default:
                        using (var service = new T())
                        {
                            service.Start();
                            Console.WriteLine(
                                "Running {0}, press any key to stop", serviceName);
                            Console.ReadKey();
                        }
                        break;
                }
                //using (var service = new T())
                //{
                //    service.Start();
                //    Console.WriteLine("Running {0}, press any key to stop", serviceName);
                //    Console.ReadKey();
                //}
            }
            else
            {
                ServiceBase.Run(new BasicService<T> { ServiceName = serviceName });
            }
        }
    }
}