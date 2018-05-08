using System;

namespace WireJunky.ServiceFramework
{
    public interface IService : IDisposable
    {
        void Start();
    }
}