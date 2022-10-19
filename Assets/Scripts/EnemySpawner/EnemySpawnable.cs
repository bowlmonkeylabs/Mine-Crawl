using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class EnemySpawnable : MonoBehaviour
    {
        #region Inspector

        [SerializeField] public bool DoCountTowardsSpawnCap = true;
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