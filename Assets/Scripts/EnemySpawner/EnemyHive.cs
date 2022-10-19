using System;
using System.Linq;
using BML.Scripts.Utils;
using UnityEngine;
using Random = System.Random;

namespace BML.Scripts
{
    public class EnemyHive : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private GameObject _spawnPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _rotationSpreadDegrees = 360f;
        [SerializeField] private float _randomRotationDegrees = 10f;

        private EnemySpawnable _enemySpawnable;
        
        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            _enemySpawnable = GetComponent<EnemySpawnable>();
        }

        #endregion

        #region Public interface

        public void SpawnChildren(int numChildren)
        {
            var container = this.transform.parent;

            int spawnCapSuccessorIndex = UnityEngine.Random.Range(0, numChildren);

            float angleStep = _rotationSpreadDegrees / numChildren;
            
            for (int i = 0; i < numChildren; i++)
            {
                var newGameObject =
                    GameObjectUtils.SafeInstantiate(true, _spawnPrefab, container);

                var baseAngle = angleStep * i;
                var randomAngle = baseAngle + UnityEngine.Random.Range(-_randomRotationDegrees/2, _randomRotationDegrees/2);
                var randomDir = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
                var rotation = Quaternion.LookRotation(randomDir, newGameObject.transform.up);
                newGameObject.transform.SetPositionAndRotation(_spawnPoint.position, rotation);

                var newEnemySpawnable = newGameObject.GetComponent<EnemySpawnable>();
                bool isSuccssor = (i == spawnCapSuccessorIndex);
                newEnemySpawnable.DoCountTowardsSpawnCap = isSuccssor;
            }
        }

        #endregion
    }
}