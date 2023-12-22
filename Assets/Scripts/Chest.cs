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

    private int _opensCount = 0;

    void Awake() {
        this.setResourceLabelText();
    }

    public void TryOpen() {
        int resourceCost = getCurrentResourceCost();

        if(_resource.PlayerAmount >= resourceCost || _isGodMode.Value) {
            if(!_isGodMode.Value) _resource.PlayerAmount -= resourceCost;
        
            _opensCount++;

            _onSucceedOpen.Invoke();
            
            if(_limitOpens && _opensCount >= _costPerOpen.Length) {
                this.gameObject.layer = LayerMask.NameToLayer("Default");
                _onAllOpensUsed.Invoke();
            }

            this.setResourceLabelText();
            
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
