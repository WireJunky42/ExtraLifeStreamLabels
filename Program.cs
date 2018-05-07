using System;
using System.ServiceProcess;
using WireJunky.ServiceFramework;

namespace WireJunky.ExtraLife
{
    static class Program
    {
        public const string Name = "ExtraLifeStreamLabelService";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            BasicServiceStarter.Run<ExtraLifeStreamLabelsService>(Name);
        }
    }
}
