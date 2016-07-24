using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;
using System.Configuration;
using System.Diagnostics;


namespace CVSPService
{
    [RunInstaller(true)]
    public class ChemValidatorInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public ChemValidatorInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();
            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = ChemValidatorService.Service_Name;
            serviceInstaller.DisplayName = ChemValidatorService.Service_DisplayName;
            serviceInstaller.Description = ChemValidatorService.Service_Description;
            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }

    class ChemValidatorProgram
    {
        static void Main(string[] args)
        {
			//EventLogTraceListener eventListener = new EventLogTraceListener("ChemValidator");
			//ConsoleTraceListener cons = new ConsoleTraceListener();
			//Trace.Listeners.Add(eventListener);
			//Trace.Listeners.Add(cons);
            Trace.TraceInformation("Application ChemValidator started");

            var usage = new List<string>();
            var actions = new Dictionary<string, Action<string[]>>();
			
            InstallContext context = new InstallContext(null, args);
            ChemValidatorService svc = new ChemValidatorService(context);

            actions.Add("run", s => svc.start());
            usage.Add("run - starts the service running");
            string data_path = ConfigurationManager.AppSettings["data_path"];
            if ( String.IsNullOrEmpty(data_path) )
                throw new Exception("Mandatory \"data_path\" parameter is not set in configuration file");

            if (args.Length == 0)
            {
                ServiceBase.Run(svc);
            }
            else
            {
                if (context.Parameters["command"] == null)
                {
                    Console.Error.WriteLine(@"Usage: .\chemvalidator.exe /command=[command] [params]");
                }
                else
                {
                    string cmd = context.Parameters["command"];
                    if (actions.ContainsKey(cmd))
                    {
                        actions[cmd](args);
                    }
                    else
                    {
                        Console.Error.WriteLine(String.Join(Environment.NewLine,
                            from u in usage select @".\chemvalidator.exe /command=" + u));
                    }
                }
            }
        }
    }
}
