using System;

namespace MyCodeExample.Collections.SystemManager
{
    public interface IManager
    {
    }
    
    public interface IManagerInitializable : IDisposable
    {
        public void Initialize();
    }
}