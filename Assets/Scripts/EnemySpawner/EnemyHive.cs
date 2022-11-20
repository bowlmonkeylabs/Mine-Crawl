using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
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
        [SerializeField] private TransformSceneReference _playerRef;
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

            float angleStep;

            if (numChildren == 1)
                angleStep = _rotationSpreadDegrees / 2f;
            
            else
                angleStep = _rotationSpreadDegrees / (numChildren - 1);
            

            for (int i = 0; i < numChildren; i++)
            {
                var newGameObject =
                    GameObjectUtils.SafeInstantiate(true, _spawnPrefab, container);

                var baseAngle = angleStep * i;
                var randomAngle = baseAngle + UnityEngine.Random.Range(-_randomRotationDegrees/2, _randomRotationDegrees/2);
                var randomDir = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
                
                //Rotate random Dir so it starts away from player instead of Vector3.right
                var awayFromPlayerDir = (transform.position - _playerRef.Value.position).xoz().normalized;
                var awayFromPlayerRot = Quaternion.FromToRotation(Vector3.right, awayFromPlayerDir);
                var randomDirRelPlayerAway = awayFromPlayerRot * randomDir;
                
                //Rotate the start dir clockwise by half the rotationSpreadDegrees
                var childStartDir = Quaternion.AngleAxis(_rotationSpreadDegrees / 2f, Vector3.up) * randomDirRelPlayerAway;

                var rotation = Quaternion.LookRotation(childStartDir, Vector3.up);
                newGameObject.transform.SetPositionAndRotation(_spawnPoint.position, rotation);

                var newEnemySpawnable = newGameObject.GetComponent<EnemySpawnable>();
                bool isSuccssor = (i == spawnCapSuccessorIndex);
                newEnemySpawnable.DoCountForSpawnCap = isSuccssor;
            }
        }

        #endregion
    }
}