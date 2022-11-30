using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2.Objects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    public class EnemyInfluenceUpdater : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private DynamicGameEvent _onEnemyKilled;

        #endregion

        #region Unity lifecycle
        
        

        #endregion

        #region Public interface

        public void OnDeath()
        {
            var payload = new EnemyKilledPayload
            {
                Position = this.transform.position
            };
            _onEnemyKilled.Raise(payload);
        }

        #endregion
    }
}