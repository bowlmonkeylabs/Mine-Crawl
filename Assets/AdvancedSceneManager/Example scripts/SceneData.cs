using System;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using UnityEngine;

namespace AdvancedSceneManager.Examples
{

    public static class SceneData
    {

        const string key1 = "MyKey1";
        const string key2 = "MyKey2";

        [Serializable]
        class ComplexType
        {
            public string name;
            public Vector2 position;
        }

        static Scene scene => SceneManager.assets.scenes.Find("SampleScene");

        static void SetData()
        {
            SceneDataUtility.Set(scene, key1, "This is a string");
            SceneDataUtility.Set(scene, key2, new ComplexType() { name = "test", position = Vector2.one });
        }

        static void UnsetData()
        {
            SceneDataUtility.Unset(scene, key1);
            SceneDataUtility.Unset(scene, key2);
        }

        static void LogData()
        {
            var sceneData = SceneDataUtility.Enumerate<ComplexType>(key2);
            foreach (var scene in sceneData)
                Debug.Log("The following data is associated with '" + scene.scene.name + "':\n\n" + JsonUtility.ToJson(scene.data));
        }

    }

}
