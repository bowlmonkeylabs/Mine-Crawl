using System.Collections;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using Lazy.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Examples
{

    public class ScenePreload : MonoBehaviour
    {

        public Scene SceneToPreload;

        #region Coroutine

        //This is returned from SceneOperation, we'll use this to finish or discard.
        PreloadedSceneHelper preloadedScene;

        //Flag to make sure we don't wait for preloadedScene to be set, when
        //StartPreloadCoroutine() hasn't even been called yet
        bool hasStartedPreload;

        public void StartPreloadCoroutine()
        {

            Coroutine().StartCoroutine();
            IEnumerator Coroutine()
            {

                //Don't preload if we're already started,
                //or scene is already open (ASM does not support duplicate scenes)
                if (hasStartedPreload || SceneToPreload.isOpen)
                    yield break;

                //Flag to let us know in FinishPreloadCoroutine() if we've actually started or not.
                hasStartedPreload = true;

                //Start preload.
                //In order to retrieve preloadedScene, we need a reference to operation,
                //so assigning return value (SceneOperation<PreloadedSceneHelper>) is a must.
                var operation = SceneToPreload.Preload();
                yield return operation;

                //Get the preloadedScene
                preloadedScene = operation.value;

            }

        }

        public void FinishPreloadCoroutine()
        {

            Coroutine().StartCoroutine();
            IEnumerator Coroutine()
            {

                //Make sure we don't wait for preloadedScene to get a value
                //while preload hasn't even started yet.
                if (!hasStartedPreload)
                    yield break;

                //Wait for preloadedScene to be set, this how we know scene
                //has been preloaded, and is ready.
                yield return new WaitUntil(() => preloadedScene != null);

                //Finish actual preload
                yield return preloadedScene.FinishLoading();

                //Reset state
                hasStartedPreload = false;
                preloadedScene = null;

            }

        }

        public void DiscardPreloadCoroutine()
        {

            Coroutine().StartCoroutine();
            IEnumerator Coroutine()
            {

                //Make sure we don't wait for preloadedScene to get a value
                //while preload hasn't even started yet.
                if (!hasStartedPreload)
                    yield break;

                //Wait for preloadedScene to be set, this how we know scene
                //has been preloaded, and is ready.
                yield return new WaitUntil(() => preloadedScene != null);

                //Discard
                yield return preloadedScene.Discard();

                //Reset state
                hasStartedPreload = false;
                preloadedScene = null;

            }

        }

        #endregion
        #region Static

        //Note that PreloadedScene is null when no scene is preloaded, and won't have a value until scene is ready to finish preload.
        //Checking isStillPreloaded might be a bit reduntant, since SceneManager.standalone.preloadedScene will be set to null when
        //preload is finished, but lets check it for good measure (it'll be more useful when its set to a local variable).
        public bool hasPreloadedScene => SceneManager.standalone.preloadedScene?.isStillPreloaded ?? false;

        public void StartPreloadStatically() => SceneToPreload.Preload();
        public void FinishPreloadStatically() => SceneManager.standalone.preloadedScene?.FinishLoading();
        public void DiscardPreloadStatically() => SceneManager.standalone.preloadedScene?.Discard();

        #endregion

    }

}