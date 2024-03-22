using System;
using MyCodeExample.Collections.SystemManager;
using MyCodeExample.Game.Events;
using MyCodeExample.Game.UI.Screens;
using MyCodeExample.Managers;

namespace MyCodeExample.Game.Features
{
    public sealed class StartupScreenFeature : Feature, IFeatureInitializable
    {
        private StartupScreen _startupScreen;
        private UIScreenManager _screenManager;

        void IFeatureInitializable.Initialize()
        {
            SystemManager.TryGetManager(out _screenManager);

            SystemManager.EventBus.RegisterListenOnce<InitialPageContentReadyEvent>(OnDataReceived);
            _startupScreen = _screenManager.Get<StartupScreen>();

            _startupScreen.Show();
        }

        private void OnDataReceived(InitialPageContentReadyEvent obj)
        {
            _startupScreen.Hide();
            _screenManager.Get<PageScreen>().Show();
        }

        void IDisposable.Dispose()
        {
        }
    }
}