using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [CreateAssetMenu(fileName = "GameObjectSceneReference", menuName = "BML/SceneReferences/GameObjectSceneReference", order = 0)]
    public class GameObjectSceneReference : SceneReference<GameObject> {}
}

