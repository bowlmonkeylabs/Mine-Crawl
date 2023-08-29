using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class EnemySpawnable : MonoBehaviour
    {
        #region Inspector

        [SerializeField] public bool DoCountForSpawnCap = true;
        [SerializeField, ReadOnly] public SpawnPoint SpawnPoint;
        [SerializeField] private UnityEvent _onDespawn;
        
        #endregion

        #region Public interface
        
        public void Despawn()
        {
            _onDespawn?.Invoke();

            if (SpawnPoint != null)
            {
                SpawnPoint.RecordEnemyDespawned();
            }
            
            Destroy(this.gameObject);
        }

        public void OnDeath()
        {
            if (SpawnPoint != null)
            {
                SpawnPoint.RecordEnemyDied();
            }
        }

        #endregion

    }
}