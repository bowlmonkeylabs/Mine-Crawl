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
        [SerializeField] private TimerReference _pickaxeSwingCooldown;
        [SerializeField] private IntReference _pickaxeDamage;
        [SerializeField] private DamageType _damageType;
        
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

        [TitleGroup("Rope")]
        [SerializeField] private GameObject _ropePrefab;
        [SerializeField] private float _ropeThrowForce;
        [SerializeField] private Transform _ropeInstanceContainer;
        [SerializeField] private IntReference _inventoryRopeCount;
        
        [TitleGroup("Health")]
        [SerializeField] private Health _healthController;
        [SerializeField] private IntReference _health;
        [SerializeField] private IntReference _maxHealth;

        [TitleGroup("Combat State")]
        [SerializeField] private BoolVariable _inCombat;
        [SerializeField] private TimerVariable _combatTimer;

        [TitleGroup("Store")]
        [SerializeField] private BoolReference _isStoreOpen;
        [SerializeField] private GameEvent _onStoreFailOpen;

        [TitleGroup("GodMode")]
        [SerializeField] private BoolVariable _isGodModeEnabled;

        private bool pickaxeInputHeld = false;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _isGodModeEnabled.Subscribe(SetGodMode);
            _health.Subscribe(ClampHealth);
            _combatTimer.SubscribeFinished(SetNotInCombat);
            
            SetGodMode();
        }

        private void OnDisable()
        {
            _isGodModeEnabled.Unsubscribe(SetGodMode);
            _health.Unsubscribe(ClampHealth);
            _combatTimer.UnsubscribeFinished(SetNotInCombat);
        }

        private void Update()
        {
            if (pickaxeInputHeld) TryUsePickaxe();
            HandleHover();
            _combatTimer.UpdateTime();
            _pickaxeSwingCooldown.UpdateTime();
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

        private void OnThrowRope(InputValue value)
        {
            if (value.isPressed)
            {
                TryThrowRope();
            }
        }
        
        #endregion

        #region Pickaxe
        
        private void TryUsePickaxe()
        {
            RaycastHit hit;

            if (_pickaxeSwingCooldown.IsStarted && !_pickaxeSwingCooldown.IsFinished)
            {
                return;
            }
            
            _onUsePickaxe.Raise();
            _pickaxeSwingCooldown.RestartTimer();
            
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance,
                _interactMask, QueryTriggerInteraction.Ignore))
            {
                PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver == null) return;

                HitInfo pickaxeHitInfo = new HitInfo(_damageType, _pickaxeDamage.Value, _mainCamera.forward, hit.point);
                interactionReceiver.ReceiveInteraction(pickaxeHitInfo);
            }
            
        }

        public void SetPickaxeDistance(float dist)
        {
            _interactDistance = dist;
        }

        #endregion

        #region Throw

        private void Throw(float force, GameObject prefabToThrow, Transform container) {
            // Calculate throw
            var throwDir = _mainCamera.forward;
            var throwForce = throwDir * force;
            
            // Instantiate throwable
            var newGameObject = GameObjectUtils.SafeInstantiate(true, prefabToThrow, container);
            newGameObject.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
            var throwable = newGameObject.GetComponentInChildren<Throwable>();
            throwable.DoThrow(throwForce);
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

            this.Throw(_torchThrowForce, _torchPrefab, _torchInstanceContainer);
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

            this.Throw(_bombThrowForce, _bombPrefab, _bombInstanceContainer);
        }
        
        #endregion

        #region Rope

        private void TryThrowRope()
        {
            // Check torch count
            if (_inventoryRopeCount.Value <= 0)
            {
                return;
            }
            _inventoryRopeCount.Value -= 1;
            
            this.Throw(_ropeThrowForce, _ropePrefab, _ropeInstanceContainer);
        }
        
        #endregion

        #region Combat State

        public void SetInCombat()
        {
            _inCombat.Value = true;
            _isStoreOpen.Value = false;
            _combatTimer.StartTimer();
        }

        private void SetNotInCombat() {
            _inCombat.Value = false;
            _combatTimer.ResetTimer();
        }
        
        #endregion

        #region Store

        public void OnToggleStore()
		{
            if(!_isStoreOpen.Value && _inCombat.Value) {
                _onStoreFailOpen.Raise();
                return;
            }

            _isStoreOpen.Value = !_isStoreOpen.Value && !_inCombat.Value;
		}
        
        #endregion

        #region God Mode

        private void SetGodMode()
        {
            _healthController.SetInvincible(_isGodModeEnabled.Value);
        }

        #endregion

        private void HandleHover()
        {
            if (lastHoverUpdateTime + 1f / _hoverUpdatesPerSecond > Time.time)
                return;

            lastHoverUpdateTime = Time.time;
            
            RaycastHit hit;
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance,
                _interactMask, QueryTriggerInteraction.Ignore))
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
        
        #region Health

        public int SetHealth(int value)
        {
            return _healthController.SetHealth(value);
        }

        public void Revive()
        {
            _healthController.Revive();
        }
        
        private void ClampHealth()
        {
            if (_health.Value > _maxHealth.Value)
                _health.Value = _maxHealth.Value;
        }
        
        #endregion

    }
}