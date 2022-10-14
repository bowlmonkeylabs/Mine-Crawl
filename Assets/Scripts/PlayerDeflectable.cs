using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class PlayerDeflectable : MonoBehaviour
    {
        [SerializeField] private BoolVariable _upgradeDeflectProjectiles;
        [SerializeField] private UnityEvent<HitInfo> _onDeflectSuccess;
        [SerializeField] private UnityEvent<HitInfo> _onDeflectFailure;

        public void TryDeflect(HitInfo hitInfo)
        {
            if (_upgradeDeflectProjectiles.Value)
                _onDeflectSuccess.Invoke(hitInfo);
            else
                _onDeflectFailure.Invoke(hitInfo);
        }
    }
}