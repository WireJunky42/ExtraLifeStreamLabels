using System;
using System.Diagnostics;
using System.ServiceProcess;
using NLog;

namespace WireJunky.ServiceFramework
{
    public class BasicService<T> : ServiceBase where T : IService, new()
    {
        private IService _service;
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStart(string[] args)
        {
            _logger.Info($"Serviceprocess.BasicService.OnStart()");
            _service = new T();
            _service.Start();
        }

        protected override void OnStop()
        {
            _logger.Info($"Serviceprocess.BasicService.OnStop()");
            _service.Dispose();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return _service.HandlePowerEvent(powerStatus);
        }
    }
}