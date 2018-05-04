using System.ServiceProcess;

namespace WireJunky.ExtraLife.ExtraLifeStreamLabels
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ExtraLifeStreamLabels()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
