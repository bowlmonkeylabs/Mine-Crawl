using Pathfinding;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.SceneReferences;

public class BMLAIDestinationSetter : AIDestinationSetter
{
    [SerializeField]
    private TransformSceneReference transformSceneReference;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void ResetTargetToReference()
    {
        target = transformSceneReference.Value;
    }
}
