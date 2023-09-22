using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.ValueTypeVariables
{
    public class ValueTypeVariable<T> : Variable<T> where T : struct
    {
        public override T Value
        {
            get => runtimeValue;
            set
            {
                if (_enableLogs) Debug.Log($"SetValue {this.name}");
                
                prevValue = runtimeValue;
                runtimeValue = value;
                this.InvokeOnUpdateDelta(prevValue, runtimeValue);
                this.InvokeOnUpdate();
                if(EnableDebugOnUpdate)
                    Debug.LogError($"name: {name} | prevValue: {prevValue} | currentValue: {runtimeValue}");

                prevValue = runtimeValue;
                // Setting this means that 'prevValue' technically only contains the previous value during the frame an update occurs, otherwise it is in sync with 'runtimeValue'
                // But this allows delta updates to work even when we edit values through the inspector (which circumvents this Value.set() method)
            }
        }

        public override void Reset()
        {
            base.Reset();
        }
    }

    public class ValueTypeReference<T, VT> : Reference<T, VT> where VT : Variable<T> where T : struct
    {
        
    }
}