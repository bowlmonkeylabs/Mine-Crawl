using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.UI;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using Pathfinding;
using Sirenix.OdinInspector;

namespace BML.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField, FoldoutGroup("Interactable hover")] private Transform _mainCamera;
        [SerializeField, FoldoutGroup("Interactable hover")] private UiAimReticle _uiAimReticle;
        [SerializeField, FoldoutGroup("Interactable hover")] private int _hoverUpdatesPerSecond = 20;
        private float lastHoverUpdateTime;

        [SerializeField, FoldoutGroup("Pickaxe")] private float _interactDistance = 5f;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _interactMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _terrainMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private BoxCollider _sweepCollider;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeSwingCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeSweepCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private IntReference _pickaxeDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private IntReference _sweepDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private DamageType _damageType;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _swingPickaxeFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _swingHitFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _missSwingFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _hitTerrainFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepReadyFeedback;
        
        [SerializeField, FoldoutGroup("Torch")] private GameObject _torchPrefab;
        [SerializeField, FoldoutGroup("Torch")] private float _torchThrowForce;
        [SerializeField, FoldoutGroup("Torch")] private Transform _torchInstanceContainer;
        [SerializeField, FoldoutGroup("Torch")] private IntReference _inventoryTorchCount;
        
        [SerializeField, FoldoutGroup("Bomb")] private GameObject _bombPrefab;
        [SerializeField, FoldoutGroup("Bomb")] private float _bombThrowForce;
        [SerializeField, FoldoutGroup("Bomb")] private Transform _bombInstanceContainer;
        [SerializeField, FoldoutGroup("Bomb")] private IntReference _inventoryBombCount;

        [SerializeField, FoldoutGroup("Rope")] private GameObject _ropePrefab;
        [SerializeField, FoldoutGroup("Rope")] private float _ropeThrowForce;
        [SerializeField, FoldoutGroup("Rope")] private Transform _ropeInstanceContainer;
        [SerializeField, FoldoutGroup("Rope")] private IntReference _inventoryRopeCount;
        
        [SerializeField, FoldoutGroup("Health")] private Health _healthController;
        [SerializeField, FoldoutGroup("Health")] private IntReference _health;
        [SerializeField, FoldoutGroup("Health")] private IntReference _maxHealth;

        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _inCombat;
        [SerializeField, FoldoutGroup("Combat State")] private TimerVariable _combatTimer;

        [SerializeField, FoldoutGroup("Store")] private BoolReference _isStoreOpen;
        [SerializeField, FoldoutGroup("Store")] private GameEvent _onStoreFailOpen;

        [SerializeField, FoldoutGroup("GodMode")] private BoolVariable _isGodModeEnabled;

        private bool pickaxeInputHeld = false;
        private bool secondaryInputHeld = false;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _isGodModeEnabled.Subscribe(SetGodMode);
            _health.Subscribe(ClampHealth);
            _combatTimer.SubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.SubscribeFinished(SweepReadyFeedbacks);
            
            SetGodMode();
        }

        private void OnDisable()
        {
            _isGodModeEnabled.Unsubscribe(SetGodMode);
            _health.Unsubscribe(ClampHealth);
            _combatTimer.UnsubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.UnsubscribeFinished(SweepReadyFeedbacks);
        }

        private void Update()
        {
            if (pickaxeInputHeld) TryUsePickaxe();
            if (secondaryInputHeld) TryUseSweep();
            HandleHover();
            _combatTimer.UpdateTime();
            _pickaxeSwingCooldown.UpdateTime();
            _pickaxeSweepCooldown.UpdateTime();
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
                secondaryInputHeld = true;
                TryUseSweep();
            }
            else
            {
                secondaryInputHeld = false;
            }
        }
        
        private void OnThrowTorch(InputValue value)
        {
            TryPlaceTorch();
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
            
            _swingPickaxeFeedback.StopFeedbacks();
            _swingPickaxeFeedback.PlayFeedbacks();
            _pickaxeSwingCooldown.RestartTimer();
            
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance,
                _interactMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.IsInLayerMask(_terrainMask))
                {
                    _hitTerrainFeedback.transform.position = hit.point;
                    _hitTerrainFeedback.PlayFeedbacks();
                }
                else
                {
                    _swingHitFeedbacks.transform.position = hit.point;
                    _swingHitFeedbacks.PlayFeedbacks();
                }
                    
                
                PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver == null) return;

                HitInfo pickaxeHitInfo = new HitInfo(_damageType, _pickaxeDamage.Value, _mainCamera.forward, hit.point);
                interactionReceiver.ReceiveInteraction(pickaxeHitInfo);
            }
            else
            {
                _missSwingFeedback.PlayFeedbacks();
            }
            
        }

        public void SetPickaxeDistance(float dist)
        {
            _interactDistance = dist;
        }

        private void TryUseSweep()
        {
            if (_pickaxeSweepCooldown.IsStarted && !_pickaxeSweepCooldown.IsFinished)
            {
                return;
            }
            
            _swingPickaxeFeedback.StopFeedbacks();
            _swingPickaxeFeedback.PlayFeedbacks();
            _sweepFeedback.PlayFeedbacks();
            _pickaxeSweepCooldown.RestartTimer();
            _pickaxeSwingCooldown.RestartTimer();

            var center = _sweepCollider.transform.TransformPoint(_sweepCollider.center);
            var halfExtents = _sweepCollider.size / 2f;
            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, _sweepCollider.transform.rotation,
                _interactMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length < 1)
            {
                _missSwingFeedback.PlayFeedbacks();
                return;
            }

            // HashSet to prevent duplicates
            HashSet<PickaxeInteractionReceiver> interactionReceivers = new HashSet<PickaxeInteractionReceiver>();

            foreach (var hitCollider in hitColliders)
            {
                PickaxeInteractionReceiver interactionReceiver = hitCollider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver == null) continue;
                interactionReceivers.Add(interactionReceiver);
            }

            foreach (var interactionReceiver in interactionReceivers)
            {
                //TODO: make separate hit info for sweep (Ex. hit point does not apply)
                HitInfo pickaxeHitInfo = new HitInfo(_damageType, _sweepDamage.Value, _mainCamera.forward, 
                    interactionReceiver.transform.position);
                interactionReceiver.ReceiveInteraction(pickaxeHitInfo);
            }
        }

        private void SweepReadyFeedbacks()
        {
            _sweepReadyFeedback.PlayFeedbacks();
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