using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.UI;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using BML.Scripts.Store;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector
        [SerializeField, FoldoutGroup("Scene References")] private GameObjectSceneReference _caveGeneratorGameObjectSceneReference;
        private CaveGenComponentV2 _caveGenerator => _caveGeneratorGameObjectSceneReference?.CachedComponent as CaveGenComponentV2;
        
        [SerializeField, FoldoutGroup("Interactable hover")] private Transform _mainCamera;
        [SerializeField, FoldoutGroup("Interactable hover")] private UiAimReticle _uiAimReticle;
        [SerializeField, FoldoutGroup("Interactable hover")] private int _hoverUpdatesPerSecond = 20;
        private float lastHoverUpdateTime;

        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _interactDistance;
        [SerializeField, FoldoutGroup("Pickaxe")] private float _interactCastRadius = .25f;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _interactMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _terrainMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _enemyMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private BoxCollider _sweepCollider;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeSwingCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeSweepCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _pickaxeDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _sweepDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _swingCritChance;
        [SerializeField, FoldoutGroup("Pickaxe")] private DamageType _damageType;
        [SerializeField, FoldoutGroup("Pickaxe")] private DamageType _sweepDamageType;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _startSwingPickaxeFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _swingHitFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _missSwingFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _hitTerrainFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _hitEnemyFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepReadyFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _startSweepFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepHitFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepSuccessHitFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepHitEnemyFeedback;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _swingCritFeedbacks;
        
        [SerializeField, FoldoutGroup("Torch")] private GameObject _torchPrefab;
        [SerializeField, FoldoutGroup("Torch")] private float _torchThrowForce;
        [SerializeField, FoldoutGroup("Torch")] private Transform _torchInstanceContainer;
        [SerializeField, FoldoutGroup("Torch")] private IntReference _inventoryTorchCount;
        [SerializeField, FoldoutGroup("Torch")] private StoreItem _torchStoreItem;
        
        [SerializeField, FoldoutGroup("Bomb")] private GameObject _bombPrefab;
        [SerializeField, FoldoutGroup("Bomb")] private float _bombThrowForce;
        [SerializeField, FoldoutGroup("Bomb")] private Transform _bombInstanceContainer;
        [SerializeField, FoldoutGroup("Bomb")] private IntReference _inventoryBombCount;

        [SerializeField, FoldoutGroup("Rope")] private GameObject _ropePrefab;
        [SerializeField, FoldoutGroup("Rope")] private float _ropeThrowForce;
        [SerializeField, FoldoutGroup("Rope")] private Transform _ropeInstanceContainer;
        [SerializeField, FoldoutGroup("Rope")] private IntReference _inventoryRopeCount;
        [SerializeField, FoldoutGroup("Rope")] private StoreItem _ropeStoreItem;

        [SerializeField, FoldoutGroup("Dash")] private BoolReference _isDashActive;
        
        [SerializeField, FoldoutGroup("Health")] private Health _healthController;
        [SerializeField, FoldoutGroup("Health")] private DynamicGameEvent _tryHeal;

        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _inCombat;
        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _anyEnemiesEngaged;
        [SerializeField, FoldoutGroup("Combat State")] private float _safeCombatTimerDecayMultiplier = 2f;
        [SerializeField, FoldoutGroup("Combat State")] private TimerVariable _combatTimer;

        [SerializeField, FoldoutGroup("Store")] private BoolReference _isStoreOpen;
        [SerializeField, FoldoutGroup("Store")] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField, FoldoutGroup("Store")] private GameEvent _onStoreFailOpen;

        [SerializeField, FoldoutGroup("Upgrade Store")] private BoolReference _isUpgradeStoreOpen;

        [SerializeField, FoldoutGroup("GodMode")] private BoolVariable _isGodModeEnabled;
        
        [SerializeField, FoldoutGroup("Debug")] private BoolVariable _enableLogs;

        private bool pickaxeInputHeld = false;
        private bool secondaryInputHeld = false;
        private int totalSwingCount = 0;

        #endregion

        #region Unity lifecycle

        private void Start()
        {
            totalSwingCount = 0;
        }

        private void OnEnable()
        {
            _isGodModeEnabled.Subscribe(SetGodMode);
            _combatTimer.SubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.SubscribeFinished(SweepReadyFeedbacks);
            _tryHeal.Subscribe(Heal);
            _isDashActive.Subscribe(OnDashSetActive);
            
            SetGodMode();
        }

        private void OnDisable()
        {
            _isGodModeEnabled.Unsubscribe(SetGodMode);
            _combatTimer.UnsubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.UnsubscribeFinished(SweepReadyFeedbacks);
            _tryHeal.Unsubscribe(Heal);
            _isDashActive.Unsubscribe(OnDashSetActive);
        }

        private void Update()
        {
            if (pickaxeInputHeld) TrySwingPickaxe();
            if (secondaryInputHeld) TryUseSweep();
            HandleHover();
            _combatTimer.UpdateTime(!_anyEnemiesEngaged.Value ? _safeCombatTimerDecayMultiplier : 1f);
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
                TrySwingPickaxe();
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
        
        private void TrySwingPickaxe()
        {
            if (_pickaxeSwingCooldown.IsStarted && !_pickaxeSwingCooldown.IsFinished)
            {
                return;
            }
            
            _startSwingPickaxeFeedback.PlayFeedbacks();
            _pickaxeSwingCooldown.RestartTimer();
        }

        // To be called from animation
        public void DoSwing()
        {
            totalSwingCount++;
            
            RaycastHit hit;
            RaycastHit? lowPriorityHit = null;
            
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance.Value,
                _interactMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.IsInLayerMask(_terrainMask))
                {
                    //If hit terrain, store this hit. Will try sphereCast in search of higher priority hit
                    lowPriorityHit = hit;
                }
                else
                {
                    HandlePickaxeHit(hit);
                    return;
                }
            }

            //Don't consider terrain in sphere cast
            LayerMask interactNoTerrainMask = _interactMask & ~_terrainMask;
            if (Physics.SphereCast(_mainCamera.position, _interactCastRadius, _mainCamera.forward, out hit,
                _interactDistance.Value, interactNoTerrainMask, QueryTriggerInteraction.Ignore))
            {
                HandlePickaxeHit(hit);
                return;
            }

            // If sphere cast didn't find anything and we already stored a low priority hit, use that
            if (lowPriorityHit != null)
            {
                HandlePickaxeHit(lowPriorityHit.Value);
                return;
            }
            
            _missSwingFeedback.PlayFeedbacks();
        }
        
        private void HandlePickaxeHit(RaycastHit hit)
        {
            if (hit.collider.gameObject.IsInLayerMask(_terrainMask))
            {
                _hitTerrainFeedback.transform.position = hit.point;
                _hitTerrainFeedback.PlayFeedbacks();
            }
            else if (hit.collider.gameObject.IsInLayerMask(_enemyMask))
            {
                _hitEnemyFeedback.PlayFeedbacks();
            }

            _swingHitFeedbacks.transform.position = hit.point;
            _swingHitFeedbacks.PlayFeedbacks();
            
            
                
            PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
            if (interactionReceiver == null) return;
            
            float damage = _pickaxeDamage.Value;

            if (!_caveGenerator.SafeIsUnityNull())
            {
                Random.InitState(_caveGenerator.CaveGenParams.Seed + totalSwingCount);
            }
            else
            {
                Debug.LogWarning($"Cave generator is not assigned, so random seed is not able to be used.");
            }
            bool isCrit = Random.value < _swingCritChance.Value;

            if (isCrit)
            {
                damage *= 2f;
                _swingCritFeedbacks.PlayFeedbacks(hit.point);
            }
            HitInfo pickaxeHitInfo = new HitInfo(_damageType, Mathf.FloorToInt(damage), _mainCamera.forward, hit.point);
            interactionReceiver.ReceiveInteraction(pickaxeHitInfo);
        }

        public void SetPickaxeDistance(float dist)
        {
            _interactDistance.Value = dist;
        }
        
        private void TryUseSweep()
        {
            if (_pickaxeSweepCooldown.IsStarted && !_pickaxeSweepCooldown.IsFinished)
            {
                return;
            }
            
            _startSweepFeedback.PlayFeedbacks();
            _pickaxeSweepCooldown.RestartTimer();
            _pickaxeSwingCooldown.RestartTimer();
        }
        
        // To be called by anim
        public void DoSweep()
        {
            var center = _sweepCollider.transform.TransformPoint(_sweepCollider.center);
            var halfExtents = _sweepCollider.size / 2f;
            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, _sweepCollider.transform.rotation,
                _interactMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length < 1)
            {
                _missSwingFeedback.PlayFeedbacks();
                return;
            }
            
            _sweepSuccessHitFeedbacks.PlayFeedbacks();

            // HashSet to prevent duplicates
            HashSet<(PickaxeInteractionReceiver receiver, Vector3 colliderCenter)> interactionReceivers =
                new HashSet<(PickaxeInteractionReceiver receiver, Vector3 colliderCenter)>();

            bool isEnemyHit = false;
            foreach (var hitCollider in hitColliders)
            {
                PickaxeInteractionReceiver interactionReceiver = hitCollider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver == null)
                    continue;
                if (hitCollider.gameObject.IsInLayerMask(_enemyMask))
                    isEnemyHit = true;
                
                interactionReceivers.Add((interactionReceiver, hitCollider.bounds.center));
            }
            
            if (isEnemyHit)
                _sweepHitEnemyFeedback.PlayFeedbacks();

            foreach (var (interactionReceiver, colliderCenter) in interactionReceivers)
            {
                var hitPos = interactionReceiver.transform.position;
                
                // Use spherecast from camera to hit collider center to try approximate hit position
                var delta = colliderCenter - _mainCamera.transform.position;
                var dir = delta.normalized;
                var dist = delta.magnitude + 5f;
                RaycastHit[] hits = Physics.SphereCastAll(_mainCamera.position, .5f, dir, dist, _interactMask,
                    QueryTriggerInteraction.Ignore);

                
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject == interactionReceiver.gameObject)
                    {
                        hitPos = hit.point;
                        _sweepHitFeedbacks.PlayFeedbacks(hitPos, 1f);
                        break;
                    }
                }

                HitInfo pickaxeHitInfo = new HitInfo(_sweepDamageType, Mathf.FloorToInt(_sweepDamage.Value), _mainCamera.forward, 
                    hitPos);
                interactionReceiver.ReceiveSecondaryInteraction(pickaxeHitInfo);
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
                _onPurchaseEvent.Raise(_torchStoreItem);
                if (_inventoryTorchCount.Value <= 0)
                {
                    return;
                }
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
                _onPurchaseEvent.Raise(_ropeStoreItem);
                if (_inventoryRopeCount.Value <= 0)
                {
                    return;
                }
            }
            _inventoryRopeCount.Value -= 1;
            
            this.Throw(_ropeThrowForce, _ropePrefab, _ropeInstanceContainer);
        }
        
        #endregion

        #region Dash

        private void OnDashSetActive(bool prevValue, bool dashActive)
        {
            _healthController.SetInvincible(dashActive);
        }
        
        #endregion

        #region Combat State

        public void SetInCombat()
        {
            _inCombat.Value = true;
            _isStoreOpen.Value = false;
            _combatTimer.StartTimer();
        }

        private void SetNotInCombat() 
        {
            _inCombat.Value = false;
            _combatTimer.ResetTimer();
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
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance.Value,
                _interactMask, QueryTriggerInteraction.Ignore))
            {
                PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver != null)
                {
                    _uiAimReticle.SetReticleHover(true);
                    return;
                }
            }
            
            if (Physics.SphereCast(_mainCamera.position, _interactCastRadius, _mainCamera.forward, out hit,
                _interactDistance.Value, _interactMask, QueryTriggerInteraction.Ignore))
            {
                PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                if (interactionReceiver != null)
                {
                    _uiAimReticle.SetReticleHover(true);
                    return;
                }
            }
            
            _uiAimReticle.SetReticleHover(false);
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

        public void Heal(int amount)
        {
            _healthController.Heal(amount);
        }

        public void Heal(object p, object amount)
        {
            this.Heal((int) amount);
        }
        
        #endregion

    }
}