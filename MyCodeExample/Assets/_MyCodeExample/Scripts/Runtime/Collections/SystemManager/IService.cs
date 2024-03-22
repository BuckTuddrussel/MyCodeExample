using System;

namespace MyCodeExample.Collections.SystemManager
{
    public interface IService
    {
    }

    public interface IServiceInitializable : IDisposable
    {
        public void Initialize(Managers.SystemManager systemManager);
    }
}