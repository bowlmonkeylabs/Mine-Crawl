using Shapes;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Despawnable : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private UnityEvent _onDespawn;
        
        #endregion
        
        public void Despawn()
        {
            _onDespawn?.Invoke();
            
            Destroy(this.gameObject);
        }
    }
}