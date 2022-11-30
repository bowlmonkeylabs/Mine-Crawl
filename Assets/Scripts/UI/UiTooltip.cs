using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UiTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform _parentTransform;
    [SerializeField] private RectTransform _tooltipTransform;

    void Update() {
        var mousePos = Mouse.current.position.ReadValue();
        _tooltipTransform.gameObject.SetActive(RectTransformUtility.RectangleContainsScreenPoint(_parentTransform, mousePos));

        if(_tooltipTransform.gameObject.activeSelf) {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentTransform, Mouse.current.position.ReadValue(), null, out localPoint);
            _tooltipTransform.localPosition = localPoint;
        }
    }
}
