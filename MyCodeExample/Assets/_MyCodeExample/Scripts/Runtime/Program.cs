using System;
using UnityEngine;
using DG.Tweening;
using MyCodeExample.Managers;

namespace MyCodeExample
{
    /// <summary>
    /// The main entry point of the application, equivalent to Main in pure C# applications.
    /// </summary>
    public static class Program
    {
        private static SystemManager _systemManager;
        
        /// <summary>
        /// Gets the system manager instance.
        /// </summary>
        /// <returns>The system manager instance.</returns>
        public static SystemManager GetSystemManager() => _systemManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Main()
        {
            Debug.Log("Entering PlayMode");
            Application.quitting += OnApplicationQuit;
            
            DOTween.Init();
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
            
            _systemManager = new SystemManager();
        }

        private static void OnApplicationQuit()
        {
            Application.quitting -= OnApplicationQuit;
            
            try
            {
                ((IDisposable)_systemManager).Dispose();
                _systemManager = null;
                Environment.ExitCode = 0;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Environment.ExitCode = 1;
            }
            
            Debug.Log("Exiting PlayMode");
        }
    }
}