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

            foreach(Collider col in enemyColliders)
            {
                col.gameObject.GetComponent<Alertable>().SetAlerted();
            }
        }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            // Only draw if object is in the top-level of the selected (i.e. NOT a child of the selected object)
            if (UnityEditor.Selection.activeGameObject != gameObject) return;
            #endif
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, _miningEnemyAlertRadius.Value);
        }
    }
}