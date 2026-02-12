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
        [SerializeField] private GameObject _resourceLabelRoot;
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
            UpdateResourceLabel();
        }

        public void TryOpen() 
        {
            int resourceCost = getCurrentResourceCost();

            bool canAfford = _resource.PlayerAmount >= resourceCost || _isGodMode.Value;
            if (canAfford)
            {
                // Disable chest interaction immediately. Will reset if/after close animation.
                SetInteractable(false);

                _resource.PlayerAmount -= resourceCost; // Always subtract cost even in god mode for testing, but can always afford in god mode so wont prevent opening.
            
                _opensCount++;

                _onSucceedOpen.Invoke();
                
                bool canOpenAgain = !_limitOpens || _opensCount < _costPerOpen.Length;
                if (canOpenAgain)
                {
                    _onClose.Invoke(); // Invoke close event to allow for chest to be opened again
                }
                else
                {
                    _onAllOpensUsed.Invoke();
                }
                
                return;
            }

            _onFailOpen.Invoke();
        }

        public void SetInteractable(bool interactable)
        {
            // Change layer so it cant be interacted with anymore
            var layerName = interactable ? "Interactable" : "Default";
            gameObject.layer = LayerMask.NameToLayer(layerName);

            // Hide label if not interactable. Label will be re-shown if chest becomes interactable again and cost is greater than 0.
            UpdateResourceLabel(!interactable);
        }

        private void UpdateResourceLabel(bool forceHide = false)
        {
            int resourceCost = getCurrentResourceCost();

            // Show label if cost is greater than 0. Hide if 0.
            bool showLabel = resourceCost > 0;
            _resourceLabelRoot.SetActive(showLabel);

            // Update label if showing.
            if (showLabel)
            {
                _resourceLabelText.text = $"{resourceCost} {_resource.IconText}";
            }
        }

        private int getCurrentResourceCost() {
            return _limitOpens ? _costPerOpen[Mathf.Clamp(_opensCount, 0, _costPerOpen.Length-1)] : _resourceCost;
        }
    }
}
