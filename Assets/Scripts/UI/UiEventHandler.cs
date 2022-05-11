using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BML.Scripts.UI
{
    public class UiEventHandler : MonoBehaviour, ISubmitHandler, ICancelHandler, IPointerEnterHandler
    {
        #region Inspector
        private static string GetGroupHeaderString(string groupTitle, GameObject propagateTarget, UnityEvent fireEvent)
        {
            string propagate = !propagateTarget.SafeIsUnityNull() ? "|PROPAGATE" : "";
            string evt = fireEvent.GetPersistentEventCount() > 0 ? "|EVENT" : "";
            return $"{groupTitle} {propagate} {evt}";
        }
        
        private string onSubmitGroupTitle => GetGroupHeaderString("On Submit", _propagateSubmit, _onSubmit);
        [FoldoutGroup("$onSubmitGroupTitle", expanded: false), LabelText("Propagate Handler")]
        [SerializeField] private GameObject _propagateSubmit;
        [FoldoutGroup("$onSubmitGroupTitle"), HideLabel]
        [SerializeField] private UnityEvent _onSubmit;
        private ISubmitHandler submitHandler;
        
        private string onCancelGroupTitle => GetGroupHeaderString("On Cancel", _propagateCancel, _onCancel);
        [FoldoutGroup("$onCancelGroupTitle", expanded: false), LabelText("Propagate Handler")]
        [SerializeField] private GameObject _propagateCancel;
        [FoldoutGroup("$onCancelGroupTitle"), HideLabel]
        [SerializeField] private UnityEvent _onCancel;
        private ICancelHandler cancelHandler;
        
        private string onPointerEnterGroupTitle => GetGroupHeaderString("On Pointer Enter", _propagatePointerEnter, _onPointerEnter);
        [FoldoutGroup("$onPointerEnterGroupTitle", expanded: false), LabelText("Propagate Handler")]
        [SerializeField] private GameObject _propagatePointerEnter;
        [FoldoutGroup("$onPointerEnterGroupTitle"), HideLabel]
        [SerializeField] private UnityEvent _onPointerEnter;
        private IPointerEnterHandler pointerEnterHandler;
        #endregion
        
        #region Unity lifecycle
        private void Awake()
        {
            if (_propagateSubmit != null)
            {
                submitHandler = _propagateSubmit?.GetComponent<ISubmitHandler>();
            }

            if (_propagateCancel != null)
            {
                cancelHandler = _propagateCancel?.GetComponent<ICancelHandler>();
            }

            if (_propagatePointerEnter != null)
            {
                pointerEnterHandler = _propagatePointerEnter?.GetComponent<IPointerEnterHandler>();
            }
        }
        #endregion

        #region Unity event handlers
        public void OnSubmit(BaseEventData eventData)
        {
            _onSubmit?.Invoke();
            submitHandler?.OnSubmit(eventData);
        }
        
        public void InvokeOnSubmit()
        {
            _onSubmit?.Invoke();
        }

        public void OnCancel(BaseEventData eventData)
        {
            _onCancel?.Invoke();
            cancelHandler?.OnCancel(eventData);
        }
        
        public void InvokeOnCancel()
        {
            _onCancel?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onPointerEnter?.Invoke();
            pointerEnterHandler?.OnPointerEnter(eventData);
        }
        #endregion
    }
}