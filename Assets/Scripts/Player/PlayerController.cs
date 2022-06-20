using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.InputSystem;

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

        #region Unity lifecycle

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
                    InteractionReceiver interactionReceiver = hit.collider.GetComponent<InteractionReceiver>();
                    if (interactionReceiver == null) return;

                    Ore.OreHitInfo oreHitInfo = new Ore.OreHitInfo()
                    {
                        Damage = _pickaxeDamage.Value,
                        HitPositon = hit.point
                    };
                    interactionReceiver.ReceiveInteraction(oreHitInfo);
                }
            }
        }
    }
}