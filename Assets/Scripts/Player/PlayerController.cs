using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.UI;
using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Pathfinding;
using Sirenix.OdinInspector;

namespace BML.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector
        
        [TitleGroup("Interactable hover")]
        [SerializeField] private Transform _mainCamera;
        [SerializeField] private UiAimReticle _uiAimReticle;
        [SerializeField] private int _hoverUpdatesPerSecond = 20;
        private float lastHoverUpdateTime;

        [TitleGroup("Pickaxe")]
        [SerializeField] private GameEvent _onUsePickaxe;
        [SerializeField] private float _interactDistance = 5f;
        [SerializeField] private LayerMask _interactMask;
        [SerializeField] private EvaluateCurveVariable _pickaxeSwingCooldown;
        [SerializeField] private IntReference _pickaxeDamage;
        
        [TitleGroup("Torch")]
        [SerializeField] private GameObject _torchPrefab;
        [SerializeField] private float _torchThrowForce;
        [SerializeField] private Transform _torchInstanceContainer;
        [SerializeField] private IntReference _inventoryTorchCount;
        
        [TitleGroup("Bomb")]
        [SerializeField] private GameObject _bombPrefab;
        [SerializeField] private float _bombThrowForce;
        [SerializeField] private Transform _bombInstanceContainer;
        [SerializeField] private IntReference _inventoryBombCount;
        
        [TitleGroup("Health")]
        [SerializeField] private IntReference _health;
        [SerializeField] private IntReference _maxHealth;

        [TitleGroup("GodMode")]
        [SerializeField] private BoolVariable _isGodModeEnabled;

        private float lastSwingTime = Mathf.NegativeInfinity;
        private bool pickaxeInputHeld = false;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _isGodModeEnabled.Subscribe(SetGodMode);
            _health.Subscribe(ClampHealth);
            SetGodMode();
        }

        private void OnDisable()
        {
            _isGodModeEnabled.Unsubscribe(SetGodMode);
            _health.Unsubscribe(ClampHealth);
        }

        private void Update()
        {
            if (pickaxeInputHeld) TryUsePickaxe();
            HandleHover();
        }

        #endregion

        #region Input callbacks
        
        private void OnPrimary(InputValue value)
        {
            if (value.isPressed)
            {
                pickaxeInputHeld = true;
                TryUsePickaxe();
            }
            else
            {
                pickaxeInputHeld = false;
            }
        }

        private void OnSecondary(InputValue value)
        {
            if (value.isPressed)
            {
                TryPlaceTorch();
            }
        }

        private void OnThrowBomb(InputValue value)
        {
            if (value.isPressed)
            {
                TryThrowBomb();
            }
        }
        
        #endregion

        #region Pickaxe
        
        private void TryUsePickaxe()
        {
            RaycastHit hit;

            if (lastSwingTime + _pickaxeSwingCooldown.Value > Time.time)
                return;
            
            _onUsePickaxe.Raise();
            lastSwingTime = Time.time;
            
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance,
                _interactMask, QueryTriggerInteraction.Collide))
            {
                PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver == null) return;

                HitInfo pickaxeHitInfo = new HitInfo(_pickaxeDamage.Value, _mainCamera.forward, hit.point);
                interactionReceiver.ReceiveInteraction(pickaxeHitInfo);
            }
            
        }

        #endregion
        
        #region Torch

        private void TryPlaceTorch()
        {
            // Check torch count
            if (_inventoryTorchCount.Value <= 0)
            {
                return;
            }
            _inventoryTorchCount.Value -= 1;
            
            // Calculate throw
            var throwDir = _mainCamera.forward;
            var throwForce = throwDir * _torchThrowForce;
            
            // Instantiate torch
            var newGameObject = GameObjectUtils.SafeInstantiate(true, _torchPrefab, _torchInstanceContainer);
            newGameObject.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
            var newGameObjectRb = newGameObject.GetComponentInChildren<Rigidbody>();
            newGameObjectRb.AddForce(throwForce, ForceMode.Impulse);
        }
        
        #endregion
        
        #region Bomb

        private void TryThrowBomb()
        {
            // Check torch count
            if (_inventoryBombCount.Value <= 0)
            {
                return;
            }
            _inventoryBombCount.Value -= 1;
            
            // Calculate throw
            var throwDir = _mainCamera.forward;
            var throwForce = throwDir * _bombThrowForce;
            
            // Instantiate torch
            var newGameObject = GameObjectUtils.SafeInstantiate(true, _bombPrefab, _bombInstanceContainer);
            newGameObject.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
            var newGameObjectRb = newGameObject.GetComponentInChildren<Rigidbody>();
            newGameObjectRb.AddForce(throwForce, ForceMode.Impulse);
        }
        
        #endregion

        #region God Mode

        private void SetGodMode()
        {
            foreach(Damageable damageable in GetComponents<Damageable>()) {
                damageable.SetInvincible(_isGodModeEnabled.Value);
            }
        }

        #endregion

        private void HandleHover()
        {
            if (lastHoverUpdateTime + 1f / _hoverUpdatesPerSecond > Time.time)
                return;

            lastHoverUpdateTime = Time.time;
            
            RaycastHit hit;
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance, _interactMask, QueryTriggerInteraction.Collide))
            {
                PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver == null) return;

                _uiAimReticle.SetReticleHover(true);
            }
            else
            {
                _uiAimReticle.SetReticleHover(false);
            }
        }

        private void ClampHealth()
        {
            if (_health.Value > _maxHealth.Value)
                _health.Value = _maxHealth.Value;
        }

    }
}