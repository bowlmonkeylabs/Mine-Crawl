using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class GameObjectUtils
    {
        /// <summary>
        /// Checks if a GameObject is in a LayerMask
        /// </summary>
        /// <param name="obj">GameObject to test</param>
        /// <param name="layerMask">LayerMask with all the layers to test against</param>
        /// <returns>True if in any of the layers in the LayerMask</returns>
        public static bool IsInLayerMask(this GameObject obj, LayerMask layerMask)
        {
            // Convert the object's layer to a bitfield for comparison
            int objLayerMask = (1 << obj.layer);
            if ((layerMask.value & objLayerMask) > 0)  // Extra round brackets required!
                return true;
            else
                return false;
        }

        /// <summary>
        /// Since PrefabUtility is only accessible in UnityEditor namespace, this provides a single method which can work with Prefabs when in Editor, but also won't break in build.
        /// </summary>
        /// <param name="instanceAsPrefab"></param>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject SafeInstantiate(bool instanceAsPrefab, GameObject prefab, Transform parent = null)
        {
            GameObject newGameObject;
#if UNITY_EDITOR
            if (instanceAsPrefab)
            {
                var newObject = PrefabUtility.InstantiatePrefab(prefab, parent);
                newGameObject = newObject as GameObject;
            }
            else
            {
                newGameObject = GameObject.Instantiate(prefab, parent);
            }
#else
            newGameObject = GameObject.Instantiate(roomPrefab, clayContainer.transform);
#endif
            return newGameObject;
        }
    }
}

