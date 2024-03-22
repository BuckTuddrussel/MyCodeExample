using System;
using System.Collections.Generic;
using MyCodeExample.Managers;
using QuickPixel;

namespace MyCodeExample.Collections.SystemManager
{
    public interface IFeatureInitializable : IDisposable
    {
        public void Initialize();
    }

    public interface IFeatureEnableable
    {
        public bool FeatureAutoEnable { get; }
        public void OnFeatureEnable();
        public void OnFeatureDisable();
    }

    public interface IFeatureUpdateable
    {
        void Update();
    }

    internal sealed class FeatureUpdateManager : PlayerLoopUpdateManager<IFeatureUpdateable>
    {
        protected override Type UpdateAfterSystem => typeof(UnityEngine.PlayerLoop.Update);

        protected override void OnUpdate(IReadOnlyList<IFeatureUpdateable> updatableFeatures)
        {
            var featuresCount = updatableFeatures.Count;

            if (featuresCount == 0) return;
            
            var updatableFeaturesArray = updatableFeatures.GetInternalArray();
            for (var index = 0; index < featuresCount; index++)
            {
                updatableFeaturesArray[index].Update();
            }
        }
    }

    public abstract class Feature
    {
        protected Managers.SystemManager SystemManager { get; private set; }

        private FeatureUpdateManager _featureUpdateManager;
        private IFeatureEnableable _featureEnableable;
        private IFeatureUpdateable _featureUpdateable;

        private bool _isEnabled;

        public bool IsUpdatable { get; private set; }
        public bool IsEnablable { get; private set; }

        public bool IsEnabled => !IsEnablable || _isEnabled;

        internal void Initialize_Internal(Managers.SystemManager systemManager,
            FeatureUpdateManager featureUpdateManager)
        {
            SystemManager = systemManager;
            _featureUpdateManager = featureUpdateManager;

            if (this is IFeatureUpdateable featureUpdateable)
            {
                _featureUpdateable = featureUpdateable;
                IsUpdatable = true;
            }

            if (this is IFeatureEnableable enableable)
            {
                _featureEnableable = enableable;
                IsEnablable = true;
            }

            if (this is IFeatureInitializable featureInitializable)
            {
                featureInitializable.Initialize();
            }

            if (IsEnablable && _featureEnableable.FeatureAutoEnable)
            {
                Enable();
            }
            else if (!IsEnablable && IsUpdatable)
            {
                _featureUpdateManager.Register(_featureUpdateable);
            }
        }

        public void Enable()
        {
            if (IsEnablable && !_isEnabled)
            {
                if (IsUpdatable) _featureUpdateManager.Register(_featureUpdateable);
                _featureEnableable.OnFeatureEnable();
                _isEnabled = true;
            }
        }

        public void Disable()
        {
            if (IsEnablable && _isEnabled)
            {
                if (IsUpdatable) _featureUpdateManager.Unregister(_featureUpdateable);
                _featureEnableable.OnFeatureDisable();
                _isEnabled = false;
            }
        }

        internal void Deinitialize_Internal()
        {
            Disable();
            if(!IsEnablable && IsUpdatable) _featureUpdateManager.Unregister(_featureUpdateable);
        }

        public void Destroy() => SystemManager.DestroyFeature(this);
    }
}