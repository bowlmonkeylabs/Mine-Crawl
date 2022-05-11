using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "ParticleSystemSceneReference", menuName = "BML/SceneReferences/ParticleSystemSceneReference", order = 0)]
    public class ParticleSystemSceneReference : SceneReference<ParticleSystem> {}
}

