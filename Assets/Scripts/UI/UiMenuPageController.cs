﻿using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiMenuPageController : MonoBehaviour, ISubmitHandler, ICancelHandler
    {
        [SerializeField] private GameObject _root;
        
        [SerializeField] private Selectable _defaultSelected;
        [SerializeField] private Selectable _defaultSelectedFallback;
        [SerializeField] private UnityEvent _selectDefault;
        
        [SerializeField] private UiMenuPageController _previousPage;
        [SerializeField] private UiMenuPageController _defaultPage;
        
        private EventSystem eventSystem;
        private GameObject lastSelected;
        
        [TitleGroup("Optional Controls")]
        [SerializeField] private BoolVariable _isOpen;
        [SerializeField] private GameObject _backdropButtonPrefab;
        [SerializeField] private Transform _backdropParent;

        private GameObject backdropButtonInstance;
        
        [TitleGroup("UI Events")]
        [SerializeField] private UnityEvent _onSubmit;
        [SerializeField] private UnityEvent _onCancel;
        [SerializeField] private UnityEvent _onClose;
        [SerializeField] private UnityEvent _onOpen;
        [SerializeField] private Button.ButtonClickedEvent _onClickBackdrop;

        public Selectable DefaultSelected
        {
            get => _defaultSelected;
            set => _defaultSelected = value;
        }

        public void LogMessage(string message)
        {
            Debug.Log(message);
        }
        
        #region Unity lifecycle

        private void Awake()
        {
            eventSystem = FindObjectOfType<EventSystem>();

            SelectDefault();

            // if (_isOpen != null)
            // {
            //     TryOpenPage();
            //     _isOpen?.Subscribe(TryOpenPage);
            // }
            
            InitializeBackdrop();
        }

        private void Start()
        {
            if (_isOpen != null)
            {
                TryOpenPage();
                _isOpen?.Subscribe(TryOpenPage);
            }
        }

        private void OnDestroy()
        {
            if (_isOpen != null)
            {
                _isOpen?.Unsubscribe(TryOpenPage);
            }
        }

        #endregion

        #region Page control

        public void OpenPage()
        {
            OpenPage_SelectOption();
        }

        private void OpenPage_SelectOption(bool selectDefault = true)
        {
            if (_isOpen != null)
            {
                _isOpen.Value = true;
            }
            else
            {
                OpenPage_Internal(selectDefault);
            }
        }

        private void OpenPage_Internal(bool selectDefault = true)
        {
            if (_previousPage != null)
            {
                _previousPage.ClosePage();
            }
            _root.SetActive(true);
            if (selectDefault)
            {
                SelectDefault();
            }
            else
            {
                SelectLastSelected();
            }

            if (!_defaultPage.SafeIsUnityNull())
            {
                _defaultPage.OpenPage();
            }

            _onOpen?.Invoke();
        }

        public void ClosePage()
        {
            if (_isOpen != null)
            {
                _isOpen.Value = false;
            }
            else
            {
                ClosePage_Internal();
            }
        }

        private void ClosePage_Internal()
        {
            UpdateLastSelected();
            _root.SetActive(false);
            if (_previousPage != null)
            {
                _previousPage.OpenPage_SelectOption(false);
            }
            
            if (_defaultPage != null)
            {
                _defaultPage.ClosePage();
            }

            _onClose?.Invoke();
        }

        private void TryOpenPage()
        {
            if (_isOpen == null) return;
            if (_isOpen.Value == true)
            {
                OpenPage_Internal();
            }
            else
            {
                ClosePage_Internal();
            }
        }
        
        #endregion
        
        #region Element control

        public void SelectDefault()
        {
            bool tryUseFallback = (_defaultSelected == null || _defaultSelected.SafeIsUnityNull() || !_defaultSelected.gameObject.activeInHierarchy);
            var selectObject = (tryUseFallback ? (_defaultSelectedFallback ?? _defaultSelected) : _defaultSelected);
            if (selectObject != null && !selectObject.SafeIsUnityNull())
            {
                selectObject.Select();
            }

            _selectDefault?.Invoke();

            UpdateLastSelected();
        }

        public void SelectLastSelected()
        {
            if (!lastSelected.SafeIsUnityNull())
            {
                eventSystem.SetSelectedGameObject(lastSelected);
            }
            else
            {
                SelectDefault();
            }
        }

        private void UpdateLastSelected()
        {
            lastSelected = eventSystem?.currentSelectedGameObject;
            // Debug.Log($"{this?.name} | UpdateLastSelected | {lastSelected?.name}");
        }

        #endregion
        
        #region UI Events

        public void OnSubmit(BaseEventData eventData)
        {
            _onSubmit?.Invoke();
        }

        public void OnCancel(BaseEventData eventData)
        {
            _onCancel?.Invoke();
        }
        
        #endregion
        
        private void InitializeBackdrop()
        {
            if (_backdropButtonPrefab == null) return;
            var backdropParent = (!_backdropParent.SafeIsUnityNull() ? _backdropParent : (!_root.SafeIsUnityNull() ? _root.transform : this.transform));
            backdropButtonInstance = GameObject.Instantiate(_backdropButtonPrefab, backdropParent);
            backdropButtonInstance.transform.SetAsFirstSibling();
            var button = backdropButtonInstance.GetComponent<Button>();
            button.onClick = _onClickBackdrop;
        }
    }
}