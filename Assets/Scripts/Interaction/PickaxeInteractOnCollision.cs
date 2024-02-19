using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class PickaxeInteractOnCollision : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _interactMask;
    [SerializeField] private float _interactCooldown = 2;
    [SerializeField] private PickaxeInteractType _pickaxeInteractType;
    [SerializeField] private DamageType _damageType;
    [SerializeField] private bool _useRigidbodyVelocity = false;
    [SerializeField, ShowIf("_useRigidbodyVelocity")] private Rigidbody _rigidbody;
    [SerializeField] private UnityEvent _onCollisionEnter;

    private float _lastInteractTime = Mathf.NegativeInfinity;

    [System.Serializable]
    enum PickaxeInteractType
    {
        PRIMARY,
        SECONDARY
    }

    public void UpdateDamage(IntVariable newDamage) {
        _damage = newDamage.Value;
    }
        
    private void OnCollisionEnter(Collision col)
    {
        AttemptDamage(col.collider);
    }

    private void AttemptDamage(Collider other)
    {
        if(_lastInteractTime + _interactCooldown > Time.time) {
            return;
        }
            
        GameObject otherObj = other.gameObject;
        if (!otherObj.IsInLayerMask(_interactMask)) return;

        PickaxeInteractionReceiver receiver = otherObj.GetComponent<PickaxeInteractionReceiver>();
            
        if (receiver == null && other.attachedRigidbody != null)
            receiver = other.attachedRigidbody.GetComponent<PickaxeInteractionReceiver>();
            
        if (receiver != null) {
            var hitDirection = (_useRigidbodyVelocity ? _rigidbody.velocity : other.transform.position - transform.position).normalized;
            if (_pickaxeInteractType == PickaxeInteractType.PRIMARY)
                receiver.ReceiveInteraction(new HitInfo(_damageType, _damage, hitDirection));
            else
                receiver.ReceiveSecondaryInteraction(new HitInfo(_damageType, _damage, hitDirection));
            _lastInteractTime = Time.time;
        }
            
        _onCollisionEnter.Invoke();
    }
}
