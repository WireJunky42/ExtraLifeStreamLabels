using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace WireJunky.ServiceFramework
{
    public class BasicServiceInstaller
    {
        public static void Install(string serviceName)
        {
            CreateInstaller(serviceName).Install(new Hashtable());
        }

        public static void Uninstall(string serviceName)
        {
            CreateInstaller(serviceName).Uninstall(null);
        }

        private static Installer CreateInstaller(string serviceName)
        {
            var installer = new TransactedInstaller();
            var value = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                DisplayName = serviceName,
                ServiceName = serviceName
            };
            installer.Installers.Add(value);
            installer.Installers.Add(new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            });
            var installContext = new InstallContext(serviceName + ".install.log", null);
            installContext.Parameters["assemblypath"] = Assembly.GetEntryAssembly().Location;
            installer.Context = installContext;
            return installer;
        }
    }
}