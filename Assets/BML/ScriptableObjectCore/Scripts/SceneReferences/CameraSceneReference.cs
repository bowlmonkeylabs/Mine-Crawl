using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [CreateAssetMenu(fileName = "CameraSceneReference", menuName = "BML/SceneReferences/CameraSceneReference", order = 0)]
    public class CameraSceneReference : SceneReference<Camera> {}
}
