using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts
{
    public class EnemyDeleteTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyMask;
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.IsInLayerMask(_enemyMask))
            {
                Destroy(other.gameObject);
            }
        }
    }
}