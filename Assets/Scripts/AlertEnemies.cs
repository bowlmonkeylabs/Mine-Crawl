using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts
{
    public class AlertEnemies : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private FloatReference _miningEnemyAlertRadius;
        
        public void Alert()
        {
            Collider[] enemyColliders = Physics.OverlapSphere(transform.position, _miningEnemyAlertRadius.Value, _enemyLayerMask);
            // Debug.Log(enemyColliders.Length);

            foreach(Collider col in enemyColliders)
            {
                col.gameObject.GetComponent<EnemyController>().SetAlerted(true);
            }
        }
    }
}