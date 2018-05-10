using System;
using System.ServiceProcess;

namespace WireJunky.ServiceFramework
{
    public interface IService : IDisposable
    {
        void Start();
        bool HandlePowerEvent(PowerBroadcastStatus powerBroadcastStatus);
    }
}