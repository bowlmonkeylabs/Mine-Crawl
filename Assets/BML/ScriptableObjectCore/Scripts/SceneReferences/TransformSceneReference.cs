using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [CreateAssetMenu(fileName = "TransformSceneReference", menuName = "BML/SceneReferences/TransformSceneReference", order = 0)]
    public class TransformSceneReference : SceneReference<Transform> {}
}