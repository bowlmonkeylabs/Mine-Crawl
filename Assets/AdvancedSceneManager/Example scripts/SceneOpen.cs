using System.Collections;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Examples
{

    public class SceneOpen : MonoBehaviour
    {

        public Scene sceneToOpen;

        #region Open as standalone

        //Open as standalone (not associated with a collection)
        public void OpenStandalone()
        {
            sceneToOpen.Open();
            //Equivivalent to:
            //SceneManager.standalone.Open(sceneToOpen);
        }

        #endregion
        #region Open as collection scene

        //Open as part of the currently open collection
        public void OpenCollection()
        {
            //Will throw error if scene is not part of collection
            SceneManager.collection.Open(sceneToOpen);
        }

        #endregion
        #region Open single

        //Close all open scenes, and collection, and then open this scene
        public void OpenSingle(bool closePersistentScenes) =>
            sceneToOpen.OpenSingle(closePersistentScenes);

        #endregion
        #region Loading screen

        public void OpenWithLoadingScreen(Scene loadingScreen)
        {

            if (!loadingScreen)
            {
                //LoadingScreenUtility.fade will be null if default fade loading screen scene has been deleted / un-included from build
                loadingScreen = LoadingScreenUtility.fade;
            }

            sceneToOpen.Open().
                WithLoadingScreen(loadingScreen).
                WithLoadingScreen(use: loadingScreen); //Disables loading screen if it is null, not really needed if you already know it isn't;

        }

        #endregion
        #region Fluent api / Chaining

        public void ChainingExample()
        {

            //Open(), and other similar ASM methods, return SceneOperation.
            //SceneOperation has a fluent api that can configure it within exactly one frame of it starting (note that operations are queued, so: queue time + 1 frame). 
            sceneToOpen.Open().
                WithLoadingPriority(ThreadPriority.High). //Sets Application.backgroundLoadingPriority for the duration of the operation
                WithClearUnusedAssets(). //Calls Resources.UnloadUnusedAssets() after all scenes have been loaded / unloaded
                WithCallback(Callback.AfterLoadingScreenOpen().Do(() => Debug.Log("Loading screen opened."))).
                WithCallback(Callback.After(Phase.LoadScenes).Do(DoStuffInCoroutine));

            //Note that all callbacks are still called, even if there no loading screen or any scenes loaded

        }

        IEnumerator DoStuffInCoroutine()
        {
            //ASM will wait for this coroutine to finish before continuing normal operation
            yield return new WaitForSeconds(1);
        }

        #endregion

        public void Toggle() => sceneToOpen.Toggle();
        public void EnsureOpen() => sceneToOpen.EnsureOpen();

    }

}
