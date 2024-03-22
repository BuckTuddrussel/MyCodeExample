using System;
using QuickPixel.ScreenManager;
using MyCodeExample.Collections.SystemManager;

namespace MyCodeExample.Managers
{
    public sealed class UIScreenManager : ScreenManager, IManager, IManagerInitializable
    {
        void IManagerInitializable.Initialize() => Initialize();

        void IDisposable.Dispose() => Deinitialize();
    }
}