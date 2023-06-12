using System.Collections;
using AdvancedSceneManager.Callbacks;
using AdvancedSceneManager.Utility;
using UnityEngine;
using scene = UnityEngine.SceneManagement.Scene;
using sceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AdvancedSceneManager.Examples
{

    /// <summary>An example of how to override scene loading.</summary>
    static class SceneLoadOverride
    {

        [RuntimeInitializeOnLoadMethod]
        static void RegisterOverride()
        {

            //Register override, only one can be active, and only last one registered will used
            //SceneManager.utility.OverrideSceneLoad(LoadSceneNormal, UnloadSceneNormal);
            //SceneManager.utility.OverrideSceneLoad(LoadSceneAsync, UnloadSceneAsync);
            //SceneManager.utility.OverrideSceneLoad(LoadSceneSync, UnloadSceneSync);
            //SceneManager.utility.OverrideSceneLoad(LoadSceneNetwork, UnloadSceneNetwork);

        }

        #region Normal ASM

        static IEnumerator LoadSceneNormal(SceneLoadOverrideArgs e)
        {
            //yield break will cause ASM to load scene like normal
            yield break;
        }

        static IEnumerator UnloadSceneNormal(SceneUnloadOverrideArgs e)
        {
            //yield break will cause ASM to unload scene like normal
            yield break;
        }

        #endregion
        #region Async

        //This is how ASM currently implements scene loading

        static IEnumerator LoadSceneAsync(SceneLoadOverrideArgs e)
        {

            //Logs error and calls e.NotifyComplete(handled: true)
            //if scene is not actually included in build,
            //which means we can just break then.
            //Remove this if the scene isn't supposed to be in build list, like addressable scenes
            if (!e.CheckIsIncluded())
                yield break;

            //Load scene additively
            //If non-additive is needed, then keep in mind your collections are setup for this, ASM will get weird otherwise
            yield return
                sceneManager.LoadSceneAsync(e.scene.path, UnityEngine.SceneManagement.LoadSceneMode.Additive). //Load scene additively
                WithProgress(e.ReportProgress); //Reports progress to loading screens

            //Get the scene that was opened with UnityEngine.SceneManagement.LoadScene,
            //since unity does not support this for some reason
            var scene = e.GetOpenedScene();

            //Notify that we're complete, if we don't call this
            //then ASM will run its regular action
            e.SetCompleted(scene);

        }

        static IEnumerator UnloadSceneAsync(SceneUnloadOverrideArgs e)
        {
            yield return sceneManager.UnloadSceneAsync(e.unityScene).WithProgress(e.ReportProgress);
            e.SetCompleted();
        }

        #endregion
        #region Sync

        //UnityEngine.SceneManagement.SceneManager.UnloadScene() is deprecated
#pragma warning disable CS0618 // Type or member is obsolete

        static IEnumerator LoadSceneSync(SceneLoadOverrideArgs e)
        {

            //Logs error and calls e.NotifyComplete(handled: true)
            //if scene is not actually included in build,
            //which means we can just break then.
            //Remove this if the scene isn't supposed to be in build list, like addressable scenes.
            if (!e.CheckIsIncluded())
                yield break;

            //Setup event handler in order to retrieve scene once loaded
            scene? loadedScene = null;
            sceneManager.sceneLoaded += SceneManager_sceneLoaded;
            void SceneManager_sceneLoaded(scene scene, UnityEngine.SceneManagement.LoadSceneMode _)
            {
                loadedScene = scene;
                sceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            }

            //Load scene additively
            //If non-additive is needed, then keep in mind your collections are setup for this, ASM will get weird otherwise
            sceneManager.LoadScene(e.scene.path, UnityEngine.SceneManagement.LoadSceneMode.Additive);

            //Keep in mind that this will never return if there is an error occurs when loading scene
            yield return new WaitUntil(() => loadedScene.HasValue);

            //Notify that we're complete, if we don't call this
            //then ASM will run its regular action
            e.SetCompleted(loadedScene.Value);

        }

        static IEnumerator UnloadSceneSync(SceneUnloadOverrideArgs e)
        {

            //Setup event handler in order to make sure we wait until scene is actually unloaded.
            //ASM does not really care whatever scene is unloaded or not, but loading screens might
            //disappear to early if we don't
            bool hasUnloaded = false;
            sceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            void SceneManager_sceneUnloaded(scene scene)
            {
                hasUnloaded = true;
                sceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
            }

            //Unload the scene
            if (!sceneManager.UnloadScene(e.unityScene))
                Debug.LogError("Scene could not be unloaded.");

            //Keep in mind that this will never return if there is an error occurs when loading scene
            yield return new WaitUntil(() => hasUnloaded);

            //Scene is probably closed, but hierarchy might still display it,
            //so lets wait for it to update for good measure
            yield return null;

            //Notify ASM that we have completed, and normal action should not run
            e.SetCompleted();

        }

#pragma warning restore CS0618

        #endregion

    }

}
