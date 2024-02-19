using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.Player.Items;
using BML.Scripts.Player.Items.Loot;
using BML.Scripts.UI;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts.Player
{
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        #region Inspector

        [SerializeField, FoldoutGroup("Player components")] private KinematicCharacterMotor _kinematicCharacterMotor;
        [SerializeField, FoldoutGroup("Player components")] private FallDamageController _fallDamageController;
        [SerializeField, FoldoutGroup("Player components")] private PlayerInput playerInput;
        
        [SerializeField, FoldoutGroup("Interactable hover")] private Transform _mainCamera;
        [SerializeField, FoldoutGroup("Interactable hover")] private UiAimReticle _uiAimReticle;
        [SerializeField, FoldoutGroup("Interactable hover")] private int _hoverUpdatesPerSecond = 20;
        [SerializeField, FoldoutGroup("Interactable hover")] private float _aimReticleNearScalingDistance = 0.1f;
        [SerializeField, FoldoutGroup("Interactable hover")] private AnimationCurve _aimReticleNearScalingCurve = AnimationCurve.Constant(0, 1, 1);
        private float lastHoverUpdateTime;

        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _interactDistance;
        [SerializeField, FoldoutGroup("Pickaxe")] private float _interactCastRadius = .25f;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _interactMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _terrainMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private LayerMask _enemyMask;
        [SerializeField, FoldoutGroup("Pickaxe")] private BoxCollider _sweepCollider;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeSwingCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeSweepCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private TimerReference _pickaxeThrowCooldown;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _pickaxeDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _sweepDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _swingCritChance;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _sweepCritChance;
        [SerializeField, FoldoutGroup("Pickaxe")] private int _critDamageMultiplier = 2;
        [SerializeField, FoldoutGroup("Pickaxe")] private GameEvent _onSwingPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe")] private DynamicGameEvent _onSwingPickaxeHit;
        [SerializeField, FoldoutGroup("Pickaxe")] private DynamicGameEvent _onSwingPickaxeCrit;
        [SerializeField, FoldoutGroup("Pickaxe")] private GameEvent _onSweepPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe")] private GameEvent _onSweepPickaxeHit;
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
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _startPickaxeThrowFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _startPickaxeReceiveFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _swingCritFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepCritFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepCritInstanceFeedbacks;

        [SerializeField, FoldoutGroup("Dash")] private BoolReference _isDashActive;
        [SerializeField, FoldoutGroup("Dash")] private SafeFloatValueReference _postDashInvincibilityTime;
        
        [SerializeField, FoldoutGroup("Health")] private Health _healthController;
        [SerializeField, FoldoutGroup("Health")] private Health _healthTemporaryController;
        [SerializeField, FoldoutGroup("Health")] private DynamicGameEvent _tryHeal;
        [SerializeField, FoldoutGroup("Health")] private DynamicGameEvent _tryHealTemporary;

        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _inCombat;
        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _anyEnemiesEngaged;
        [SerializeField, FoldoutGroup("Combat State")] private float _safeCombatTimerDecayMultiplier = 2f;
        [SerializeField, FoldoutGroup("Combat State")] private TimerVariable _combatTimer;

        [SerializeField, FoldoutGroup("Store")] private BoolReference _isStoreOpen;
        [SerializeField, FoldoutGroup("Store")] private DynamicGameEvent _onPurchaseEvent;

        [SerializeField, FoldoutGroup("Inventory")] private DynamicGameEvent _onReceivePickup;
        [SerializeField, FoldoutGroup("Inventory")] private PlayerInventory _playerInventory;
        [SerializeField, FoldoutGroup("Inventory")] private ItemPickupSpawner _inventoryItemDropper;

        [SerializeField, FoldoutGroup("Experience")] private IntReference _playerExperience;
        [SerializeField, FoldoutGroup("Experience")] private CurveVariable _levelExperienceCurve;
        [SerializeField, FoldoutGroup("Experience")] private IntReference _playerCurrentLevel;
        [SerializeField, FoldoutGroup("Experience")] private IntReference _availableUpdateCount;
        [SerializeField, FoldoutGroup("Experience")] private FloatReference _requiredExperience;
        [SerializeField, FoldoutGroup("Experience")] private FloatReference _previousRequiredExperience;

        [SerializeField, FoldoutGroup("GodMode")] private BoolVariable _isGodModeEnabled;
        
        [SerializeField, FoldoutGroup("Debug")] private bool _enableLogs;

        private InputAction primaryAction;
        private InputAction secondaryAction;
        private bool secondaryInputHeld = false;
        private bool pickaxeHeld = true;
        private PickaxeInteractionReceiver hoveredInteractionReceiver = null;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _isGodModeEnabled.Subscribe(SetGodMode);
            _combatTimer.SubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.SubscribeFinished(SweepReadyFeedbacks);
            _pickaxeThrowCooldown.SubscribeFinished(StartPickaxeReceiveFeedbacks);
            _tryHeal.Subscribe(Heal);
            _tryHealTemporary.Subscribe(HealTemporary);
            _isDashActive.Subscribe(OnDashSetActive);
            _playerExperience.Subscribe(TryIncrementCurrentLevelAndAvailableUpdateCount);
            _onReceivePickup.Subscribe(ReceivePickupDynamic);
            _playerInventory.OnAnyPlayerItemReplaced += DropItemFromInventory;
            
            SetGodMode();
            primaryAction = playerInput.actions.FindAction("Primary");
            secondaryAction = playerInput.actions.FindAction("Secondary");
        }

        private void OnDisable()
        {
            _isGodModeEnabled.Unsubscribe(SetGodMode);
            _combatTimer.UnsubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.UnsubscribeFinished(SweepReadyFeedbacks);
            _pickaxeThrowCooldown.UnsubscribeFinished(StartPickaxeReceiveFeedbacks);
            _tryHeal.Unsubscribe(Heal);
            _tryHealTemporary.Unsubscribe(HealTemporary);
            _isDashActive.Unsubscribe(OnDashSetActive);
            _playerExperience.Unsubscribe(TryIncrementCurrentLevelAndAvailableUpdateCount);
            _onReceivePickup.Unsubscribe(ReceivePickupDynamic);
            _playerInventory.OnAnyPlayerItemReplaced -= DropItemFromInventory;
        }

        private void Update()
        {
            if (primaryAction.IsPressed()) TrySwingPickaxe();
            if (secondaryAction.IsPressed()) TryUsePickaxeThrow();
            HandleHover();
            HandleReticleScaling();
            _combatTimer.UpdateTime(!_anyEnemiesEngaged.Value ? _safeCombatTimerDecayMultiplier : 1f);
            _pickaxeSwingCooldown.UpdateTime();
            _pickaxeSweepCooldown.UpdateTime();
            _pickaxeThrowCooldown.UpdateTime();
        }

        #endregion

        #region Input callbacks

        
        
        #endregion

        #region Pickaxe
        
        private void TrySwingPickaxe()
        {
            if (_pickaxeSwingCooldown.IsStarted && !_pickaxeSwingCooldown.IsFinished || !pickaxeHeld)
            {
                return;
            }
            
            _startSwingPickaxeFeedback.PlayFeedbacks();
            _pickaxeSwingCooldown.RestartTimer();
        }

        // To be called from animation
        public void DoSwing()
        {
            SeedManager.Instance.UpdateSteppedSeed("PickaxeSwing");
            
            RaycastHit hit;
            RaycastHit? lowPriorityHit = null;

            _onSwingPickaxe.Raise();
            
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
            _onSwingPickaxeHit.Raise(hit.point);
                
            PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
            if (interactionReceiver == null) return;
            
            float damage = _pickaxeDamage.Value;

            Random.InitState(SeedManager.Instance.GetSteppedSeed("PickaxeSwing"));
            bool isCrit = Random.value < _swingCritChance.Value;

            if (isCrit)
            {
                damage *= _critDamageMultiplier;
                _swingCritFeedbacks.PlayFeedbacks(hit.point);
                _onSwingPickaxeCrit.Raise(hit.point);
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
            // if (_pickaxeSweepCooldown.IsStarted && !_pickaxeSweepCooldown.IsFinished)
            // {
            //     return;
            // }
            //
            // _startSweepFeedback.PlayFeedbacks();
            // _pickaxeSweepCooldown.RestartTimer();
            // _pickaxeSwingCooldown.RestartTimer();
        }
        
        // To be called by anim
        public void DoSweep()
        {
            // var center = _sweepCollider.transform.TransformPoint(_sweepCollider.center);
            // var halfExtents = _sweepCollider.size / 2f;
            // Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, _sweepCollider.transform.rotation,
            //     _interactMask, QueryTriggerInteraction.Ignore);
            //
            // _onSweepPickaxe.Raise();
            //
            // if (hitColliders.Length < 1)
            // {
            //     _missSwingFeedback.PlayFeedbacks();
            //     return;
            // }
            //
            // _sweepSuccessHitFeedbacks.PlayFeedbacks();
            // _onSweepPickaxeHit.Raise();
            //
            // // HashSet to prevent duplicates
            // HashSet<(PickaxeInteractionReceiver receiver, Vector3 colliderCenter)> interactionReceivers =
            //     new HashSet<(PickaxeInteractionReceiver receiver, Vector3 colliderCenter)>();
            //
            // bool isEnemyHit = false;
            // foreach (var hitCollider in hitColliders)
            // {
            //     PickaxeInteractionReceiver interactionReceiver = hitCollider.GetComponent<PickaxeInteractionReceiver>();
            //     if (interactionReceiver == null)
            //         continue;
            //     if (hitCollider.gameObject.IsInLayerMask(_enemyMask))
            //         isEnemyHit = true;
            //     
            //     interactionReceivers.Add((interactionReceiver, hitCollider.bounds.center));
            // }
            //
            // if (isEnemyHit)
            // {
            //     _sweepHitEnemyFeedback.PlayFeedbacks();
            // }
            //
            // Random.InitState(SeedManager.Instance.GetSteppedSeed("PickaxeSwing"));
            // bool isCrit = Random.value < _sweepCritChance.Value;
            // if (isCrit)
            // {
            //     _sweepCritFeedbacks.PlayFeedbacks();
            // }
            //
            // foreach (var (interactionReceiver, colliderCenter) in interactionReceivers)
            // {
            //     var hitPos = interactionReceiver.transform.position;
            //     
            //     // Use spherecast from camera to hit collider center to try approximate hit position
            //     var delta = colliderCenter - _mainCamera.transform.position;
            //     var dir = delta.normalized;
            //     var dist = delta.magnitude + 5f;
            //     RaycastHit[] hits = Physics.SphereCastAll(_mainCamera.position, .5f, dir, dist, _interactMask,
            //         QueryTriggerInteraction.Ignore);
            //     
            //     foreach (var hit in hits)
            //     {
            //         if (hit.collider.gameObject == interactionReceiver.gameObject)
            //         {
            //             hitPos = hit.point;
            //             if (isCrit)
            //             {
            //                 _sweepCritInstanceFeedbacks.AllowSameFramePlay();
            //                 _sweepCritInstanceFeedbacks.PlayFeedbacks(hitPos, 1f);
            //             }
            //             else
            //             {
            //                 _sweepHitFeedbacks.AllowSameFramePlay();
            //                 _sweepHitFeedbacks.PlayFeedbacks(hitPos, 1f);
            //             }
            //             break;
            //         }
            //     }
            //
            //     int damage = Mathf.FloorToInt(_sweepDamage.Value) * (isCrit ? _critDamageMultiplier : 1);
            //     HitInfo pickaxeHitInfo = new HitInfo(_sweepDamageType, damage, _mainCamera.forward, 
            //         hitPos);
            //     interactionReceiver.ReceiveSecondaryInteraction(pickaxeHitInfo);
            // }
        }

        private void TryUsePickaxeThrow()
        {
            if (!pickaxeHeld)
            {
                return;
            }
            
            _startPickaxeThrowFeedbacks.PlayFeedbacks();
            _pickaxeThrowCooldown.RestartTimer();
        }

        public void DoPickaxeThrow()
        {
            pickaxeHeld = false;
        }

        private void StartPickaxeReceiveFeedbacks()
        {
            _startPickaxeReceiveFeedbacks.PlayFeedbacks();
        }

        public void ReceivePickaxe()
        {
            pickaxeHeld = true;
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

        #region Dash

        private void OnDashSetActive(bool prevValue, bool dashActive)
        {
            if (dashActive)
            {
                SetInvincible(dashActive);
            }
            else
            {
                LeanTween.value(0f, 1f, _postDashInvincibilityTime.Value)
                    .setOnComplete(_ => SetInvincible(dashActive));
            }
        }
        
        #endregion

        #region Combat State

        public void SetInCombat()
        {
            _inCombat.Value = true;
            _isStoreOpen.Value = false;
            _combatTimer.RestartTimer();
        }

        private void SetNotInCombat() 
        {
            _inCombat.Value = false;
            _combatTimer.ResetTimer();
        }
        
        #endregion
        
        #region Pickup

        private void ReceivePickupDynamic(object prev, object curr)
        {
            ReceivePickup(curr as PlayerItem);
        }

        private void ReceivePickup(PlayerItem item)
        {
            _playerInventory.TryAddItem(item);
        }

        private void DropItemFromInventory(PlayerItem prevItem, PlayerItem newItem)
        {
            _inventoryItemDropper.SpawnItemPickup(prevItem);
        }
        
        #endregion

        #region God Mode

        private void SetGodMode()
        {
            SetInvincible(_isGodModeEnabled.Value);
        }

        #endregion

        #region Upgrades

        public void TryIncrementCurrentLevelAndAvailableUpdateCount() {
            _previousRequiredExperience.Value = CalculateRequiredExperience(_playerCurrentLevel.Value);
            _requiredExperience.Value = CalculateRequiredExperience(_playerCurrentLevel.Value + 1);
            
            //maximum check of going up 10 levels at once (they should never gain that much xp at once)
            int levelChecks = 0;

            while(_playerExperience.Value >= _requiredExperience.Value && levelChecks < 10)
            {
                AddLevel(1, false);
                levelChecks++;
            }
        }

        public float CalculateRequiredExperience(int level) {
            float requiredExperience = 0;
            for(int x = 0; x < level; x++) {
                requiredExperience += _levelExperienceCurve.Value.Evaluate(x);
            }

            return requiredExperience;
        }

        public void AddLevel(int amount, bool setXp)
        {
            if (amount > 0)
                _availableUpdateCount.Value += amount;

            _playerCurrentLevel.Value = Mathf.Max(0, _playerCurrentLevel.Value + amount);
            _previousRequiredExperience.Value = CalculateRequiredExperience(_playerCurrentLevel.Value);
            _requiredExperience.Value = CalculateRequiredExperience(_playerCurrentLevel.Value + 1);

            if (setXp)
                _playerExperience.Value = Mathf.CeilToInt(_previousRequiredExperience.Value);

        }

        #endregion

        #region Set position and teleport

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool resetFallDamage = true)
        {
            // If in play mode, move player using kinematicCharController motor to avoid race condition
            if (ApplicationUtils.IsPlaying_EditorSafe)
            {
                if(_enableLogs) Debug.Log($"Moving KCC to {position}");
                if (_kinematicCharacterMotor != null)
                {
                    _kinematicCharacterMotor.SetPositionAndRotation(position, rotation);
                    if (resetFallDamage)
                    {
                        _fallDamageController.ResetFall();
                    }
                }
                else
                {
                    this.transform.position = position;
                    Debug.LogWarning("Could not find KinematicCharacterMotor on player! " +
                                     "Moving player position directly via Transform.");
                }
            }
            else
            {
                if(_enableLogs) Debug.Log($"Moving transform to {position}");
                this.transform.position = position;
            }
        }
        
        public void TpToWaypoint(string tpPointName)
        {
            var tpPoint = TeleportUtil.FindTpPoint(tpPointName);
            if (tpPoint == null)
            {
                throw new Exception("No teleport point named " + tpPointName);
            }
            this.SetPositionAndRotation(tpPoint.position, tpPoint.rotation);
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
                    
                    if(hoveredInteractionReceiver == null) {
                        hoveredInteractionReceiver = interactionReceiver;
                        interactionReceiver.ReceiveHoverInteraction();
                    } else if(interactionReceiver != hoveredInteractionReceiver) {
                        hoveredInteractionReceiver.ReceiveUnHoverInteraction();

                        hoveredInteractionReceiver = interactionReceiver;
                        interactionReceiver.ReceiveHoverInteraction();
                    }

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

                    if(hoveredInteractionReceiver == null) {
                        hoveredInteractionReceiver = interactionReceiver;
                        interactionReceiver.ReceiveHoverInteraction();
                    } else if(interactionReceiver != hoveredInteractionReceiver) {
                        hoveredInteractionReceiver.ReceiveUnHoverInteraction();

                        hoveredInteractionReceiver = interactionReceiver;
                        interactionReceiver.ReceiveHoverInteraction();
                    }

                    return;
                }
            }

            _uiAimReticle.SetReticleHover(false);

            if(hoveredInteractionReceiver != null) {
                hoveredInteractionReceiver.ReceiveUnHoverInteraction();
                hoveredInteractionReceiver = null;
            }
        }

        private void HandleReticleScaling()
        {
            RaycastHit hit;
            LayerMask combinedLayerMask = _terrainMask | _enemyMask | _interactMask;
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _aimReticleNearScalingDistance,
                    combinedLayerMask, QueryTriggerInteraction.Ignore))
            {
                var factor = hit.distance / _aimReticleNearScalingDistance;
                var value = _aimReticleNearScalingCurve.Evaluate(factor);
                _uiAimReticle.SetReticleScaling(value);
            }
            else
            {
                _uiAimReticle.SetReticleScaling(1);
            }
        }
        
        #region Health

        public int SetHealth(int value)
        {
            return _healthController.SetHealth(value);
        }
        
        public int SetHealthTemporary(int value)
        {
            return _healthTemporaryController.SetHealth(value);
        }

        public void Revive()
        {
            _healthController.Revive();
        }

        public void Heal(int amount)
        {
            _healthController.Heal(amount);
        }
        public void HealTemporary(int amount)
        {
            _healthTemporaryController.Heal(amount);
        }

        public void Heal(object p, object amount)
        {
            this.Heal((int) amount);
        }
        
        public void HealTemporary(object p, object amount)
        {
            this.HealTemporary((int) amount);
        }

        private void SetInvincible(bool invincible) {
            invincible = invincible || _isGodModeEnabled.Value;
            _healthController.SetInvincible(invincible);
            _healthTemporaryController.SetInvincible(invincible);
        }
        
        #endregion

    }
}