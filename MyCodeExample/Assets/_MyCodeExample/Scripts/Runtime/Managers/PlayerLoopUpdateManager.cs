using System;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using QuickPixel;

namespace MyCodeExample.Managers
{
    internal abstract class PlayerLoopUpdateManager<TUpdateType> : IDisposable
    {
        private readonly List<TUpdateType> _updateables = new List<TUpdateType>();

        private PlayerLoopSystem _customPlayerLoopSystem;

        protected PlayerLoopUpdateManager()
        {
            InsertIntoPlayerLoop();
        }

        protected abstract Type UpdateAfterSystem { get; }

        public void Dispose()
        {
            var playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystem(ref playerLoopSystem, _customPlayerLoopSystem);
            PlayerLoop.SetPlayerLoop(playerLoopSystem);

            _updateables.Clear();
        }
        
        private void InsertIntoPlayerLoop()
        {
            var playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
    
            _customPlayerLoopSystem = new PlayerLoopSystem
            {
                subSystemList = null,
                updateDelegate = OnUpdate_Internal,
                type = typeof(TUpdateType),
            };

   
            AddAfterSystem(ref playerLoopSystem, _customPlayerLoopSystem, UpdateAfterSystem);
            PlayerLoop.SetPlayerLoop(playerLoopSystem);
        }
        
        public void Register(TUpdateType updateable)
        {
            _updateables.Add(updateable);
        }

        public void Unregister(TUpdateType updateable)
        {
            _updateables.Remove(updateable);
        }

        private void OnUpdate_Internal()
        {
            OnUpdate(_updateables);
        }

        protected abstract void OnUpdate(IReadOnlyList<TUpdateType> updatableFeatures);

        private static void AddAfterSystem(ref PlayerLoopSystem loopSystem, PlayerLoopSystem systemToAdd,
            Type updateAfterSystem)
        {
            var newSubSystemList = new List<PlayerLoopSystem>();

            foreach (var subSystem in loopSystem.subSystemList)
            {
                newSubSystemList.Add(subSystem);

                if (subSystem.type == updateAfterSystem)
                    newSubSystemList.Add(systemToAdd);
            }

            loopSystem.subSystemList = newSubSystemList.ToArray();
        }

        private static void RemoveSystem(ref PlayerLoopSystem loopSystem, PlayerLoopSystem systemToRemove)
        {
            var subSystemList = new List<PlayerLoopSystem>(loopSystem.subSystemList);
            subSystemList.RemoveFast(systemToRemove);
            loopSystem.subSystemList = subSystemList.ToArray();
        }
    }
}