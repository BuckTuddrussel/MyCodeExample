using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuickPixel.ScreenManager
{
    /// <summary>
    ///     Simplified version of my UI solution - might be corrupted
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        private readonly Dictionary<Type, UIObject> _registeredUIObjects = new();
        private readonly WaitForSecondsRealtime _uiRefreshInterval = new(0.33F);
        private readonly List<UIObject> UIObjectsOnScene = new(100);
        private readonly Queue<UIObject> UIObjectsToHide = new();
        private readonly Queue<UIObject> UIObjectsToShow = new();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;
        private bool _isUIDirty;

        protected void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // MK Note: I had to cut off my addressable atlas solution so here's weak alternative solution
            SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
            SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
            Register(FindObjectsOfType<UIObject>());

            StartCoroutine(UIUpdateRoutine());
        }

        private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            foreach (var rootGameObject in scene.GetRootGameObjects())
                Register(rootGameObject.GetComponentInChildren<UIObject>(true));
        }

        private void SceneManagerOnsceneUnloaded(Scene arg0)
        {
            throw new SimplifiedNotSupportedException();
        }

        internal void Register(params UIObject[] uiObjects)
        {
            foreach (var uiObject in uiObjects)
            {
                if (!_registeredUIObjects.TryAdd(uiObject.GetType(), uiObject))
                    throw new SimplifiedNotSupportedException();

                uiObject.Setup(this);
            }

            foreach (var uiObject in uiObjects) uiObject.Init();
        }

        internal void Unregister<T>(T uiObject) where T : UIObject
        {
            _registeredUIObjects.Remove(uiObject.GetType());
        }

        public T Get<T>() where T : UIObject
        {
            return _registeredUIObjects[typeof(T)] as T;
        }

        public void Show<T>(T uiObject) where T : UIObject
        {
            UIObjectsToShow.Enqueue(uiObject);
            _isUIDirty = true;
        }

        public void Hide<T>(T uiObject) where T : UIObject
        {
            UIObjectsToHide.Enqueue(uiObject);
            _isUIDirty = true;
        }

        public void DestroyUIObject<T>(T uiObject) where T : UIObject
        {
            throw new SimplifiedNotSupportedException();
        }

        // MK Note: In this state it is possible to break that loop with exception, however its possible to make it
        //          completely safe! Just implement coroutine tasks to get also better control over it (better than on regular async) 
        private IEnumerator UIUpdateRoutine()
        {
            var token = _cancellationTokenSource.Token;
            var uiUpdaterEnumerators = new List<IUIUpdaterEnumerator>
            {
                new UIHideUpdater(this),
                new UIShowUpdater(this)
            };

            while (!token.IsCancellationRequested)
            {
                if (!_isUIDirty)
                {
                    yield return _uiRefreshInterval;
                    continue;
                }

                foreach (var uiUpdaterEnumerator in uiUpdaterEnumerators)
                {
                    uiUpdaterEnumerator.UpdateQueues();
                    yield return uiUpdaterEnumerator;
                }

                _isUIDirty = false;
            }
        }

        protected void Deinitialize()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _cancellationTokenSource.Cancel();
            SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
            SceneManager.sceneUnloaded -= SceneManagerOnsceneUnloaded;
        }

        private interface IUIUpdaterEnumerator : IEnumerator
        {
            void UpdateQueues();
        }

        private struct UITransitionData
        {
            public UIObject UIObject;
            public readonly IEnumerator IEnumerator;

            public UITransitionData(UIObject uiObject, IEnumerator iEnumerator)
            {
                UIObject = uiObject;
                IEnumerator = iEnumerator;
            }
        }

        private sealed class UIShowUpdater : IUIUpdaterEnumerator
        {
            private readonly Queue<UITransitionData> _enumerators;
            private readonly ScreenManager _screenManager;

            public UIShowUpdater(ScreenManager screenManager)
            {
                _screenManager = screenManager;
                _enumerators = new Queue<UITransitionData>(50);
            }

            public object Current { get; private set; }

            void IUIUpdaterEnumerator.UpdateQueues()
            {
                while (_screenManager.UIObjectsToShow.TryDequeue(out var uiObject))
                {
                    _enumerators.Enqueue(new UITransitionData(uiObject, uiObject.OnAnimationInInner()));
                    _screenManager.UIObjectsOnScene.Add(uiObject);
                }
            }

            public bool MoveNext()
            {
                var moveNext = _enumerators.TryDequeue(out var transitionData);
                var enumerator = transitionData.IEnumerator;

                if (moveNext && enumerator.MoveNext())
                {
                    _enumerators.Enqueue(transitionData);
                    Current = enumerator.Current;
                }
                else
                {
                    Current = 0; // replace with something that's not allocating
                }

                return moveNext;
            }

            public void Reset()
            {
                throw new SimplifiedNotSupportedException();
            }
        }

        private sealed class UIHideUpdater : IUIUpdaterEnumerator
        {
            private readonly Queue<UITransitionData> _enumerators;
            private readonly ScreenManager _screenManager;

            public UIHideUpdater(ScreenManager screenManager)
            {
                _screenManager = screenManager;
                _enumerators = new Queue<UITransitionData>(50);
            }

            public object Current { get; private set; }

            void IUIUpdaterEnumerator.UpdateQueues()
            {
                while (_screenManager.UIObjectsToHide.TryDequeue(out var uiObject))
                {
                    _enumerators.Enqueue(new UITransitionData(uiObject, uiObject.OnAnimationOutInner()));
                    _screenManager.UIObjectsOnScene.Remove(uiObject);
                }
            }

            public bool MoveNext()
            {
                var moveNext = _enumerators.TryDequeue(out var transitionData);
                var enumerator = transitionData.IEnumerator;

                if (moveNext && enumerator.MoveNext())
                {
                    _enumerators.Enqueue(transitionData);
                    Current = enumerator.Current;
                }
                else
                {
                    Current = 0; // replace with something that's not allocating
                }

                return moveNext;
            }

            public void Reset()
            {
                throw new SimplifiedNotSupportedException();
            }
        }
    }

    public class SimplifiedNotSupportedException : Exception
    {
        public SimplifiedNotSupportedException() : base("This is not supported in simplified version with code cutoff")
        {
        }
    }
}