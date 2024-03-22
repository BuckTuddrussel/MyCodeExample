using UnityEngine;
using UnityEngine.SceneManagement;
using MyCodeExample.Managers;
using MyCodeExample.Game.Features;
using MyCodeExample.Game.Services;

namespace MyCodeExample.Game
{
    // This class initializes the main game features after proper scene has been loaded.
    public static class MainGame // : MonoBehaviour
    {
        // The name of the default scene to load.
        public const string DEFAULT_SCENE = "Main";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            Debug.Log("Initializing main game...");
            // Ensure that we are on the correct scene before initializing.
            var currentScene = SceneManager.GetActiveScene();
            var systemManager = Program.GetSystemManager();
            
            if (string.Equals(currentScene.name, DEFAULT_SCENE))
            {
                InitFeatures(systemManager);
            }
            else
            {
                SceneManager.LoadSceneAsync(DEFAULT_SCENE).completed += (_) => InitFeatures(systemManager);
            }
            Debug.Log("Main game initialized");
        }

        // Initialize the game features.
        private static void InitFeatures(SystemManager systemManager)
        {
            // Init services and managers first
            systemManager.AddService(new PageDataService(new MyCodeExample.Client.DataServerMock()));
            systemManager.CreateAndAddMonoManager<UIScreenManager>();

            // At the end init features
            systemManager.AddFeature(new StartupScreenFeature(), new PageScreenFeature(), new ErrorHandlingFeature());
        }
    }
}