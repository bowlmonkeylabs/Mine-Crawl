using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class EnemySpawnable : MonoBehaviour
    {
        #region Inspector

        [FormerlySerializedAs("DoCountTowardsSpawnCap")] [SerializeField] public bool DoCountForSpawnCap = true;
        [SerializeField] private UnityEvent _onDespawn;
        
        #endregion

        #region Public interface
        
        public void Despawn()
        {
            _onDespawn?.Invoke();
            
            Destroy(this.gameObject);
        }

        #endregion

    }
}