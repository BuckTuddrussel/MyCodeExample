using System;
using MyCodeExample.Collections.SystemManager;
using UnityEngine;
using MyCodeExample.Game.Events;

namespace MyCodeExample.Game.Features
{
    public sealed class ErrorHandlingFeature : Feature, IFeatureInitializable
    {
        private EventBus _eventBus;
        void IFeatureInitializable.Initialize()
        {
            _eventBus = SystemManager.EventBus;
            _eventBus.Register<SetupPageContentErrorEvent>(OnSetupPageContentError);
            _eventBus.Register<SwitchPageContentErrorEvent>(OnSwitchPageContentError);
        }

        private void OnSetupPageContentError(SetupPageContentErrorEvent _)
        {
            Debug.LogError("Setup page failed");
        }

        private void OnSwitchPageContentError(SwitchPageContentErrorEvent _)
        {
            Debug.LogError("Switch page failed");
        }
        
        void IDisposable.Dispose()
        {
        }
    }
}