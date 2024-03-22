using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCodeExample.Collections.SystemManager;

namespace MyCodeExample.Managers
{
    public sealed class SystemManager : IDisposable
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
        private readonly Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();
        private readonly HashSet<Feature> _features = new HashSet<Feature>();

        private readonly FeatureUpdateManager _updateManager = new FeatureUpdateManager();

        public readonly EventBus EventBus = new EventBus();

        public bool TryGetService<T>(out T manager) where T : class, IService
        {
            var isSuccess = _services.TryGetValue(typeof(T), out var result);

            if (isSuccess)
            {
                manager = (T)result;
            }
            else
            {
                manager = null;
            }

            return isSuccess;
        }

        public bool TryGetManager<T>(out T manager) where T : class, IManager
        {
            var isSuccess = _managers.TryGetValue(typeof(T), out var result);

            if (isSuccess)
            {
                manager = (T)result;
            }
            else
            {
                manager = null;
            }

            return isSuccess;
        }

        public void CreateAndAddMonoManager<T>() where T : MonoBehaviour, IManager, new()
        {
            var managerType = typeof(T);

            var newManagerObject = new GameObject(managerType.Name);
            var manager = (IManager)newManagerObject.AddComponent(managerType);
            UnityEngine.Object.DontDestroyOnLoad(newManagerObject);

            if (manager is IManagerInitializable initializable)
            {
                initializable.Initialize();
            }

            _managers.Add(managerType, manager);
        }

        public void AddManager<T>(IManager manager) where T : IManager, new()
        {
            var managerType = typeof(T);

            if (manager is IManagerInitializable initializable)
            {
                initializable.Initialize();
            }

            _managers.Add(managerType, manager);
        }

        public void AddService(IService feature)
        {
            var serviceType = feature.GetType();
            if (_services.ContainsKey(serviceType))
            {
                throw new AlreadyRegisteredException(feature.GetType());
            }

            if (feature is IServiceInitializable initializable)
            {
                initializable.Initialize(this);
            }

            _services.Add(serviceType, feature);
        }

        public void AddFeature(params Feature[] features)
        {
            foreach (var feature in features)
            {
                AddFeature(feature);
            }
        }

        public void AddFeature(Feature feature)
        {
            if (!_features.Add(feature))
            {
                throw new AlreadyRegisteredException(feature.GetType());
            }

            feature.Initialize_Internal(this, _updateManager);
        }

        public void DestroyFeature(Feature feature)
        {
            if (!_features.Contains(feature))
            {
                throw new NotFoundException(feature.GetType());
            }

            _features.Remove(feature);

            feature.Deinitialize_Internal();

            if (feature is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public void DestroyManager(IManager manager)
        {
            var managerType = manager.GetType();
            if (!_managers.ContainsKey(managerType))
            {
                throw new NotFoundException(managerType);
            }

            _managers.Remove(managerType);

            if (manager is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public void DestroyService(IService service)
        {
            var serviceType = service.GetType();
            if (!_services.ContainsKey(serviceType))
            {
                throw new NotFoundException(serviceType);
            }

            _services.Remove(serviceType);

            if (service is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
            var featuresArray = _features.ToArray();
            for (int i = featuresArray.Length - 1; i >= 0; i--)
            {
                DestroyFeature(featuresArray[i]);
            }

            var managersArray = _managers.Values.ToArray();
            for (int i = managersArray.Length - 1; i >= 0; i--)
            {
                DestroyManager(managersArray[i]);
            }

            var servicesArray = _services.Values.ToArray();
            for (int i = servicesArray.Length - 1; i >= 0; i--)
            {
                DestroyService(servicesArray[i]);
            }

            _updateManager.Dispose();
        }
    }
}