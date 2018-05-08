using System.ServiceProcess;

namespace WireJunky.ServiceFramework
{
    public class BasicService<T> : ServiceBase where T : IService, new()
    {
        private IService _service;

        protected override void OnStart(string[] args)
        {
            _service = new T();
            _service.Start();
        }

        protected override void OnStop()
        {
            _service.Dispose();
        }
    }
}