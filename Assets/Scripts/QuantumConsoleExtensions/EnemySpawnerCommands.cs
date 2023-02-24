using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("enemy.")]
    public class EnemySpawnerCommands : MonoBehaviour
    {
        [SerializeField] private EnemySpawnManager _enemySpawnManager;
        [SerializeField] private FloatVariable _currentPercentToMaxSpawn;
        [SerializeField] private TransformSceneReference _playerTransformSceneReference;

        #region Commands
        
        [Command("pause-spawning", "Pauses enemy spawning and difficulty curve.")]
        private void SetSpawningPaused(bool paused = true)
        {
            _enemySpawnManager.IsSpawningPaused = paused;
        }
        
        [Command("pause-despawning", "Pauses enemy despawning.")]
        private void SetDespawningPaused(bool paused = true)
        {
            _enemySpawnManager.IsDespawningPaused = paused;
        }
        
        [Command("difficulty-curve-factor", "Sets the _currentPercentToMaxSpawn of the EnemySpawnManager")]
        private void SetDifficultyCurveFactor(float factor)
        {
            _currentPercentToMaxSpawn.Value = factor;
        }

        [Command("despawn-all", "Despawns all enemies instantly.")]
        private void DespawnAll()
        {
            _enemySpawnManager.DespawnAll();
        }

        [Command("spawn-at-player", "Spawn an enemy at the player's position. Enemies are referenced by name (e.g. Zombie, Slime)")]
        private void Spawn(string enemyName, int count = 1, int health = -1)
        {
            for (int i = 0; i < count; i++)
            {
                var enemySpawnerParams = _enemySpawnManager.EnemySpawnerParamsList
                    .FirstOrDefault(p => p.SpawnAtTags.Any(t => t.Prefab.name.StartsWith(enemyName, StringComparison.OrdinalIgnoreCase)));
                var newEnemy = _enemySpawnManager.SpawnEnemyByName(enemySpawnerParams, _playerTransformSceneReference.Value.position,
                    enemyName, false, 3);

                if (health >= 0)
                {
                    var healthComponent = newEnemy.GetComponent<Health>();
                    healthComponent.SetHealth(health);
                }
                
            }
        }
        
        #endregion
    }
}