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
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _pickaxeDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _sweepDamage;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _swingCritChance;
        [SerializeField, FoldoutGroup("Pickaxe")] private SafeFloatValueReference _sweepCritChance;
        [SerializeField, FoldoutGroup("Pickaxe")] private int _critDamageMultiplier = 2;
        [SerializeField, FoldoutGroup("Pickaxe")] private GameEvent _onSwingPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe")] private GameEvent _onSwingPickaxeHit;
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
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _swingCritFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepCritFeedbacks;
        [SerializeField, FoldoutGroup("Pickaxe")] private MMF_Player _sweepCritInstanceFeedbacks;
        
        [SerializeField, FoldoutGroup("Torch")] private GameObject _torchPrefab;
        [SerializeField, FoldoutGroup("Torch")] private float _torchThrowForce;
        [SerializeField, FoldoutGroup("Torch")] private TransformSceneReference _torchInstanceContainer;
        [SerializeField, FoldoutGroup("Torch")] private TimerReference _torchCooldownTimer;
        [SerializeField, FoldoutGroup("Torch")] private IntReference _torchCount;
        [SerializeField, FoldoutGroup("Torch")] private IntReference _maxTorchCount;
        
        [SerializeField, FoldoutGroup("Bomb")] private GameObject _bombPrefab;
        [SerializeField, FoldoutGroup("Bomb")] private float _bombThrowForce;
        [SerializeField, FoldoutGroup("Bomb")] private TransformSceneReference _bombInstanceContainer;
        [SerializeField, FoldoutGroup("Bomb")] private IntReference _inventoryBombCount;

        [SerializeField, FoldoutGroup("Rope")] private GameObject _ropePrefab;
        [SerializeField, FoldoutGroup("Rope")] private float _ropeThrowForce;
        [SerializeField, FoldoutGroup("Rope")] private TransformSceneReference _ropeInstanceContainer;
        [SerializeField, FoldoutGroup("Rope")] private TimerReference _ropeCooldownTimer;
        [SerializeField, FoldoutGroup("Rope")] private IntReference _ropeCount;
        [SerializeField, FoldoutGroup("Rope")] private IntReference _maxRopeCount;

        [SerializeField, FoldoutGroup("Dash")] private BoolReference _isDashActive;
        [SerializeField, FoldoutGroup("Dash")] private SafeFloatValueReference _postDashInvincibilityTime;
        
        [SerializeField, FoldoutGroup("Health")] private Health _healthController;
        [SerializeField, FoldoutGroup("Health")] private DynamicGameEvent _tryHeal;

        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _inCombat;
        [SerializeField, FoldoutGroup("Combat State")] private BoolVariable _anyEnemiesEngaged;
        [SerializeField, FoldoutGroup("Combat State")] private float _safeCombatTimerDecayMultiplier = 2f;
        [SerializeField, FoldoutGroup("Combat State")] private TimerVariable _combatTimer;

        [SerializeField, FoldoutGroup("Store")] private BoolReference _isStoreOpen;
        [SerializeField, FoldoutGroup("Store")] private DynamicGameEvent _onPurchaseEvent;

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

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _isGodModeEnabled.Subscribe(SetGodMode);
            _combatTimer.SubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.SubscribeFinished(SweepReadyFeedbacks);
            _tryHeal.Subscribe(Heal);
            _isDashActive.Subscribe(OnDashSetActive);
            _playerExperience.Subscribe(TryIncrementCurrentLevelAndAvailableUpdateCount);
            _torchCooldownTimer.SubscribeFinished(TryIncrementTorchCount);
            _ropeCooldownTimer.SubscribeFinished(TryIncrementRopeCount);
            
            SetGodMode();
            _requiredExperience.Value = _levelExperienceCurve.Value.Evaluate(_playerCurrentLevel.Value);
            _previousRequiredExperience.Value = 0;
            primaryAction = playerInput.actions.FindAction("Primary");
            secondaryAction = playerInput.actions.FindAction("Secondary");
        }

        private void OnDisable()
        {
            _isGodModeEnabled.Unsubscribe(SetGodMode);
            _combatTimer.UnsubscribeFinished(SetNotInCombat);
            _pickaxeSweepCooldown.UnsubscribeFinished(SweepReadyFeedbacks);
            _tryHeal.Unsubscribe(Heal);
            _isDashActive.Unsubscribe(OnDashSetActive);
            _playerExperience.Unsubscribe(TryIncrementCurrentLevelAndAvailableUpdateCount);
            _torchCooldownTimer.UnsubscribeFinished(TryIncrementTorchCount);
            _ropeCooldownTimer.UnsubscribeFinished(TryIncrementRopeCount);
        }

        private void Update()
        {
            if (primaryAction.IsPressed()) TrySwingPickaxe();
            if (secondaryAction.IsPressed()) TryUseSweep();
            HandleHover();
            HandleReticleScaling();
            _combatTimer.UpdateTime(!_anyEnemiesEngaged.Value ? _safeCombatTimerDecayMultiplier : 1f);
            _pickaxeSwingCooldown.UpdateTime();
            _pickaxeSweepCooldown.UpdateTime();
            _torchCooldownTimer.UpdateTime();
            _ropeCooldownTimer.UpdateTime();
            
            
        }

        #endregion

        #region Input callbacks

        private void OnThrowTorch(InputValue value)
        {
            if (value.isPressed)
            {
                TryThrowTorch();
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
            _onSwingPickaxeHit.Raise();
                
            PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
            if (interactionReceiver == null) return;
            
            float damage = _pickaxeDamage.Value;

            Random.InitState(SeedManager.Instance.GetSteppedSeed("PickaxeSwing"));
            bool isCrit = Random.value < _swingCritChance.Value;

            if (isCrit)
            {
                damage *= _critDamageMultiplier;
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
            
            _onSweepPickaxe.Raise();

            if (hitColliders.Length < 1)
            {
                _missSwingFeedback.PlayFeedbacks();
                return;
            }
            
            _sweepSuccessHitFeedbacks.PlayFeedbacks();
            _onSweepPickaxeHit.Raise();

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
            {
                _sweepHitEnemyFeedback.PlayFeedbacks();
            }
            
            Random.InitState(SeedManager.Instance.GetSteppedSeed("PickaxeSwing"));
            bool isCrit = Random.value < _sweepCritChance.Value;
            if (isCrit)
            {
                _sweepCritFeedbacks.PlayFeedbacks();
            }

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
                        if (isCrit)
                        {
                            _sweepCritInstanceFeedbacks.AllowSameFramePlay();
                            _sweepCritInstanceFeedbacks.PlayFeedbacks(hitPos, 1f);
                        }
                        else
                        {
                            _sweepHitFeedbacks.AllowSameFramePlay();
                            _sweepHitFeedbacks.PlayFeedbacks(hitPos, 1f);
                        }
                        break;
                    }
                }

                int damage = Mathf.FloorToInt(_sweepDamage.Value) * (isCrit ? _critDamageMultiplier : 1);
                HitInfo pickaxeHitInfo = new HitInfo(_sweepDamageType, damage, _mainCamera.forward, 
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

        private void TryThrowTorch()
        {
            if(_isGodModeEnabled.Value) {
                this.Throw(_torchThrowForce, _torchPrefab, _torchInstanceContainer.Value);
                return;
            }

            if(_torchCount.Value > 0 || _isGodModeEnabled.Value) {
                this.Throw(_torchThrowForce, _torchPrefab, _torchInstanceContainer.Value);
                _torchCount.Value--;

                if(!_torchCooldownTimer.IsStarted || _torchCooldownTimer.IsFinished) {
                    _torchCooldownTimer.RestartTimer();
                }
            }
        }

        private void TryIncrementTorchCount() {
            if(_torchCount.Value < _maxTorchCount.Value) {
                _torchCount.Value += 1;
                if(_torchCount.Value < _maxTorchCount.Value) {
                    _torchCooldownTimer.RestartTimer();
                }
            }
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

            this.Throw(_bombThrowForce, _bombPrefab, _bombInstanceContainer.Value);
        }
        
        #endregion

        #region Rope

        private void TryThrowRope()
        {
            if(_isGodModeEnabled.Value) {
                this.Throw(_ropeThrowForce, _ropePrefab, _ropeInstanceContainer.Value);
                return;
            }

            if(_ropeCount.Value > 0) {
                this.Throw(_ropeThrowForce, _ropePrefab, _ropeInstanceContainer.Value);
                _ropeCount.Value--;

                if(!_ropeCooldownTimer.IsStarted || _ropeCooldownTimer.IsFinished) {
                    _ropeCooldownTimer.RestartTimer();
                }
            }
        }
        
        private void TryIncrementRopeCount() {
            if(_ropeCount.Value < _maxRopeCount.Value) {
                _ropeCount.Value += 1;
                if(_ropeCount.Value < _maxRopeCount.Value) {
                    _ropeCooldownTimer.RestartTimer();
                }
            }
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
            SetInvincible(_isGodModeEnabled.Value);
        }

        #endregion

        #region Upgrades

        public void TryIncrementCurrentLevelAndAvailableUpdateCount() {
            //maximum check of going up 10 levels at once (they should never gain that much xp at once)
            int levelChecks = 0;

            while(_playerExperience.Value >= _requiredExperience.Value && levelChecks < 10)
            {
                AddLevel(1, false);
                levelChecks++;
            }
        }

        public void AddLevel(int amount, bool setXp)
        {
            if (amount > 0)
                _availableUpdateCount.Value += amount;

            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    _previousRequiredExperience.Value = _requiredExperience.Value;
                    _requiredExperience.Value += _levelExperienceCurve.Value.Evaluate(_playerCurrentLevel.Value + 1);
                    _playerCurrentLevel.Value = Mathf.Max(0, _playerCurrentLevel.Value + 1);
                }
            }
            else
            {
                for (int i = 0; i < Mathf.Abs(amount); i++)
                {
                    _requiredExperience.Value -= _levelExperienceCurve.Value.Evaluate(_playerCurrentLevel.Value);
                    _previousRequiredExperience.Value = _requiredExperience.Value - _levelExperienceCurve.Value.Evaluate(_playerCurrentLevel.Value - 1);
                    _playerCurrentLevel.Value = Mathf.Max(0, _playerCurrentLevel.Value - 1);
                }
            }

            if (setXp)
                _playerExperience.Value = Mathf.CeilToInt(_previousRequiredExperience.Value);

        }

        #endregion

        #region Set position

        public void SetPosition(Vector3 destination, bool resetFallDamage = true)
        {
            // If in play mode, move player using kinematicCharController motor to avoid race condition
            if (ApplicationUtils.IsPlaying_EditorSafe)
            {
                if(_enableLogs) Debug.Log($"Moving KCC to {destination}");
                if (_kinematicCharacterMotor != null)
                {
                    _kinematicCharacterMotor.SetPosition(destination);
                    _fallDamageController.ResetFall();
                }
                else
                {
                    this.transform.position = destination;
                    Debug.LogWarning("Could not find KinematicCharacterMotor on player! " +
                                     "Moving player position directly via Transform.");
                }
            }
            else
            {
                if(_enableLogs) Debug.Log($"Moving transform to {destination}");
                this.transform.position = destination;
            }
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

        private void SetInvincible(bool invincible) {
            invincible = invincible || _isGodModeEnabled.Value;
            _healthController.SetInvincible(invincible);
        }
        
        #endregion

    }
}