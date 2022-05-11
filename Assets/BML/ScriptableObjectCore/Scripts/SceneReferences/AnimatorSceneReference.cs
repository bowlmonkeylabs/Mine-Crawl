using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [CreateAssetMenu(fileName = "AnimatorSceneReference", menuName = "BML/SceneReferences/AnimatorSceneReference", order = 0)]
    public class AnimatorSceneReference : SceneReference<Animator> {}
}
