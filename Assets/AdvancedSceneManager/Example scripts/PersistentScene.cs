using AdvancedSceneManager.Core;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Examples
{

    public class PersistentScene : MonoBehaviour
    {

        public void OpenPersistent(Scene scene) => scene.OpenPersistent();

        public void SetOpenSceneAsPersistent(Scene scene)
        {
            if (scene.GetOpenSceneInfo() is OpenSceneInfo openScene && openScene.unityScene.HasValue)
                PersistentUtility.Set(openScene);
        }

        public void UnsetOpenSceneAsPersistent(Scene scene)
        {
            if (scene.GetOpenSceneInfo() is OpenSceneInfo openScene && openScene.unityScene.HasValue)
                PersistentUtility.Unset(openScene);
        }

    }

}
