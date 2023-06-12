using System.Collections;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Examples
{

    public class CollectionOpen : MonoBehaviour
    {

        public SceneCollection collectionToOpen;

        #region Open

        public void Open()
        {
            collectionToOpen.Open();
            //Equivivalent to:
            //SceneManager.collection.Open(collectionToOpen);
        }

        #endregion
        #region Open with user data

        public void OpenWithUserData(ScriptableObject scriptableObject)
        {
            //Note: Overrides data set from scene manager window
            collectionToOpen.userData = scriptableObject;
            collectionToOpen.Open();
        }

        #endregion
        #region Open with loading screen

        //Overrides loading screen
        public void OpenWithLoadingScreen(Scene loadingScreen)
        {

            if (!loadingScreen)
            {
                //LoadingScreenUtility.fade will be null if default fade loading screen scene has been deleted / un-included from build
                loadingScreen = LoadingScreenUtility.fade;
            }

            collectionToOpen.Open().
                WithLoadingScreen(loadingScreen).
                WithLoadingScreen(use: loadingScreen); //Disables loading screen if it is null, not really needed if you already know it isn't

        }

        #endregion
        #region Fluent api / Chaining

        public void ChainingExample()
        {

            //Open(), and other similar ASM methods, return SceneOperation.
            //SceneOperation has a fluent api that can configure it within exactly one frame of it starting (note that operations are queued, so: queue time + 1 frame). 
            collectionToOpen.Open().
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

        public void Toggle() => collectionToOpen.Toggle();
        public void EnsureOpen() => collectionToOpen.EnsureOpen();

    }

}
