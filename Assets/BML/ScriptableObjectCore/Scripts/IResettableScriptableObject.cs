using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Managers
{
    public interface IResettableScriptableObject
    {
        public void ResetScriptableObject();
        
        public delegate void OnResetScriptableObject();

        public event OnResetScriptableObject OnReset;
    }
}
