using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class PlayerDeflectable : MonoBehaviour
    {
        [SerializeField] private BoolVariable _upgradeDeflectProjectiles;
        [SerializeField] private BoolVariable _upgradeDeflectDamage;
        [SerializeField] private BoolVariable _upgradeDeflectExplosion;
        [SerializeField] private UnityEvent<HitInfo> _onDeflectSuccess;
        [SerializeField] private UnityEvent<HitInfo> _onDeflectFailure;
        [SerializeField] private UnityEvent _onDeflectUpdateDamage;
        [SerializeField] private UnityEvent _onDeflectUpdateExplosion;

        public void TryDeflect(HitInfo hitInfo)
        {
            if (_upgradeDeflectProjectiles.Value) {
                _onDeflectSuccess.Invoke(hitInfo);
                if(_upgradeDeflectDamage.Value) {
                    _onDeflectUpdateDamage.Invoke();
                }
                if(_upgradeDeflectExplosion.Value) {
                    _onDeflectUpdateExplosion.Invoke();
                }
            }
            else {
                _onDeflectFailure.Invoke(hitInfo);
            }
        }
    }
}