using Pathfinding;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.SceneReferences;

public class BMLAIDestinationSetter : AIDestinationSetter
{
    [SerializeField]
    private TransformSceneReference transformSceneReference;

    protected override void Awake()
    {
        base.target = transformSceneReference.Value;
        base.Awake();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void ResetTargetToReference()
    {
        target = transformSceneReference.Value;
    }
}
