using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private BoolVariable _isGodMode;
        [SerializeField] private PlayerResource _resource;
        [SerializeField] private TMP_Text _resourceLabelText;

        [SerializeField] private bool _limitOpens = true;
        [SerializeField, HideIf("_limitOpens")] private int _resourceCost = 5;
        [SerializeField, ShowIf("_limitOpens")] private int[] _costPerOpen = {5};

        [SerializeField] private UnityEvent _onSucceedOpen;
        [SerializeField] private UnityEvent _onAllOpensUsed;
        [SerializeField] private UnityEvent _onFailOpen;
        [SerializeField] private UnityEvent _onClose;

        private int _opensCount = 0;

        void Awake() {
            this.setResourceLabelText();
        }

        public void TryOpen() 
        {
            int resourceCost = getCurrentResourceCost();

            bool canAfford = _resource.PlayerAmount >= resourceCost || _isGodMode.Value;
            if (canAfford)
            {
                _resource.PlayerAmount -= resourceCost; // Always subtract cost even in god mode for testing, but can always afford in god mode so wont prevent opening.
            
                _opensCount++;

                _onSucceedOpen.Invoke();
                
                bool canOpenAgain = !_limitOpens || _opensCount < _costPerOpen.Length;
                if (canOpenAgain)
                {
                    this.setResourceLabelText(); // Update label after opening to reflect new cost

                    _onClose.Invoke(); // Invoke close event to allow for chest to be opened again
                }
                else
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Default"); // Change layer so it cant be interacted with anymore

                    _onAllOpensUsed.Invoke();
                }
                
                return;
            }

            _onFailOpen.Invoke();
        }

        private void setResourceLabelText() {
            int resourceCost = getCurrentResourceCost();

            _resourceLabelText.text = $"{resourceCost} {_resource.IconText}";
        }

        private int getCurrentResourceCost() {
            return _limitOpens ? _costPerOpen[Mathf.Clamp(_opensCount, 0, _costPerOpen.Length-1)] : _resourceCost;
        }
    }
}
