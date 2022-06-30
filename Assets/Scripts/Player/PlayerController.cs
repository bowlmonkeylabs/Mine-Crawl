using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.InputSystem;
using Pathfinding;

namespace BML.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameEvent _onUsePickaxe;
        [SerializeField] private Transform _mainCamera;
        [SerializeField] private float _interactDistance = 5f;
        [SerializeField] private LayerMask _interactMask;
        [SerializeField] private TimerReference _interactCooldown;
        [SerializeField] private IntReference _pickaxeDamage;

        [SerializeField] private DynamicGameEvent _onMineOre;
        [SerializeField] private float _miningEnemyAlertRadius;
        [SerializeField] private LayerMask _enemyLayerMask;

        #region Unity lifecycle

        private void Awake() {
            _onMineOre.Subscribe((p, p2) => OnMineOre());
        }

        private void FixedUpdate()
        {
            _interactCooldown.UpdateTime();
        }

        #endregion

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

        private void OnMineOre() {
            Collider[] enemyColliders = Physics.OverlapSphere(transform.position, _miningEnemyAlertRadius, _enemyLayerMask);
            Debug.Log(enemyColliders.Length);

            foreach(Collider collider in enemyColliders) {
                collider.transform.root.GetComponentInChildren<BMLAIDestinationSetter>().enabled = true;
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, _miningEnemyAlertRadius);
        }
    }
}