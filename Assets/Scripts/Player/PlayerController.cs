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
        [SerializeField] private TimerReference _interactCooldown;
        [SerializeField] private IntReference _pickaxeDamage;

        [TitleGroup("Mine ore")]
        [SerializeField] private FloatReference _miningEnemyAlertRadius;
        
        [TitleGroup("Torch")]
        [SerializeField] private GameObject _torchPrefab;
        [SerializeField] private float _torchThrowForce;
        [SerializeField] private Transform _torchInstanceContainer;
        [SerializeField] private IntReference _inventoryTorchCount;

        #endregion

        #region Unity lifecycle

        private void Update()
        {
            HandleHover();
        }

        private void FixedUpdate()
        {
            _interactCooldown.UpdateTime();
        }
        
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, _miningEnemyAlertRadius.Value);
        }

        #endregion

        #region Input callbacks
        
        private void OnPrimary(InputValue value)
        {
            if (value.isPressed)
            {
                _interactCooldown.SubscribeFinished(TryUsePickaxe);
                TryUsePickaxe();
            }
            else
            {
                _interactCooldown.UnsubscribeFinished(TryUsePickaxe);
            }
        }

        private void OnSecondary(InputValue value)
        {
            if (value.isPressed)
            {
                TryPlaceTorch();
            }
        }
        
        #endregion

        #region Pickaxe
        
        private void TryUsePickaxe()
        {
            RaycastHit hit;
            
            if ((_interactCooldown.IsStopped || _interactCooldown.IsFinished))
            {
                _onUsePickaxe.Raise();
                _interactCooldown.RestartTimer();
                
                if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance,
                    _interactMask, QueryTriggerInteraction.Collide))
                {
                    PickaxeInteractionReceiver interactionReceiver = hit.collider.GetComponent<PickaxeInteractionReceiver>();
                    if (interactionReceiver == null) return;

                    PickaxeHitInfo pickaxeHitInfo = new PickaxeHitInfo()
                    {
                        Damage = _pickaxeDamage.Value,
                        HitPositon = hit.point
                    };
                    interactionReceiver.ReceiveInteraction(pickaxeHitInfo);
                }
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

    }
}