using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class Container : MonoBehaviour
    {
        [SerializeField] private bool _clearOnAwake = true;
        [SerializeField] private bool _clearOnLevelChange = false;
        [SerializeField, ShowIf("_clearOnLevelChange")] private GameEvent _onLevelChange;

        public void Awake() {
            if(_clearOnAwake) {
                ClearChildren();
            }
        }

        public void OnEnable() {
            if(_clearOnLevelChange) {
                _onLevelChange.Subscribe(ClearChildren);
            }
        }

        public void OnDisable() {
            if(_clearOnLevelChange) {
                _onLevelChange.Unsubscribe(ClearChildren);
            }
        }

        private void ClearChildren() {
            for(var i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
