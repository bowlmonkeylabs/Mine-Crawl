using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
        
        private string onSubmitGroupTitle => GetGroupHeaderString("On Submit", PropagateSubmit, _onSubmit);
        [FormerlySerializedAs("_propagateSubmit")]
        [FoldoutGroup("$onSubmitGroupTitle", expanded: false), LabelText("Propagate Handler")]
        [SerializeField] public GameObject PropagateSubmit;
        [FoldoutGroup("$onSubmitGroupTitle"), HideLabel]
        [SerializeField] private UnityEvent _onSubmit;
        private ISubmitHandler submitHandler;
        
        private string onCancelGroupTitle => GetGroupHeaderString("On Cancel", PropagateCancel, _onCancel);
        [FormerlySerializedAs("_propagateCancel")]
        [FoldoutGroup("$onCancelGroupTitle", expanded: false), LabelText("Propagate Handler")]
        [SerializeField] public GameObject PropagateCancel;
        [FoldoutGroup("$onCancelGroupTitle"), HideLabel]
        [SerializeField] private UnityEvent _onCancel;
        private ICancelHandler cancelHandler;
        public void OnCancelAddListener(UnityAction action) => _onCancel.AddListener(action);
        
        private string onPointerEnterGroupTitle => GetGroupHeaderString("On Pointer Enter", PropagatePointerEnter, _onPointerEnter);
        [FormerlySerializedAs("_propagatePointerEnter")]
        [FoldoutGroup("$onPointerEnterGroupTitle", expanded: false), LabelText("Propagate Handler")]
        [SerializeField] public GameObject PropagatePointerEnter;
        [FoldoutGroup("$onPointerEnterGroupTitle"), HideLabel]
        [SerializeField] private UnityEvent _onPointerEnter;
        private IPointerEnterHandler pointerEnterHandler;
        #endregion
        
        #region Unity lifecycle
        private void Awake()
        {
            if (PropagateSubmit != null)
            {
                submitHandler = PropagateSubmit?.GetComponent<ISubmitHandler>();
            }

            if (PropagateCancel != null)
            {
                cancelHandler = PropagateCancel?.GetComponent<ICancelHandler>();
            }

            if (PropagatePointerEnter != null)
            {
                pointerEnterHandler = PropagatePointerEnter?.GetComponent<IPointerEnterHandler>();
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