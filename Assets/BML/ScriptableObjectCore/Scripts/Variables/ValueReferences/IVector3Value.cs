using System;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.ValueReferences
{
    public interface IVector3Value : IValue<Vector3>
    {
        Vector3 GetVector3();
    }
}