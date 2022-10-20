using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using QFSW.QC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.PlayerLoop;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("enemy.")]
    public static class EnemySpawnerCommands
    {
        private static string _percentToMaxSpawnAddress = "Assets/ScriptableObjects/EnemySpawner/Spawner_CurrentPercentToMaxSpawn.asset";

        private static string _enemySpawnerGameObjectSceneReferenceAddress = "Assets/ScriptableObjects/EnemySpawner/Spawner_GameObjectSceneReference.asset";

        #region Commands
        
        [Command("pause-spawning", "Pauses enemy spawning and difficulty curve.")]
        private static async void SetSpawningPaused(bool paused = true)
        {
            var enemySpawnManager = await GetActiveEnemySpawnManager();

            enemySpawnManager.IsSpawningPaused = paused;
        }
        
        [Command("pause-despawning", "Pauses enemy despawning.")]
        private static async void SetDespawningPaused(bool paused = true)
        {
            var enemySpawnManager = await GetActiveEnemySpawnManager();

            enemySpawnManager.IsDespawningPaused = paused;
        }
        
        [Command("difficulty-curve-factor", "Sets the _currentPercentToMaxSpawn of the EnemySpawnManager")]
        private static async void SetDifficultyCurveFactor(float factor)
        {
            var asyncHandle = Addressables.LoadAssetAsync<FloatVariable>(_percentToMaxSpawnAddress);
            await asyncHandle.Task;

            asyncHandle.Result.Value = factor;
        }

        [Command("despawn-all", "Despawns all enemies instantly.")]
        private static async void DespawnAll()
        {
            var enemySpawner = await GetActiveEnemySpawnManager();
            
            enemySpawner.DespawnAll();
        }

        [Command("spawn-at-player", "Spawn an enemy at the player's position. Enemies are referenced by name (e.g. Zombie, Slime)")]
        private static async void Spawn(string enemyName, int count = 1, int health = -1)
        {
            var enemySpawner = await GetActiveEnemySpawnManager();
            var playerTransform = await PlayerCommands.GetActivePlayerTransform();

            for (int i = 0; i < count; i++)
            {
                var newEnemy = enemySpawner.SpawnEnemyByName(playerTransform.position, enemyName, false, true);

                if (health >= 0)
                {
                    var healthComponent = newEnemy.GetComponent<Health>();
                    healthComponent.SetHealth(health);
                }
                
            }
        }
        
        #endregion

        #region Asset access
        
        private static async Task<EnemySpawnManager> GetActiveEnemySpawnManager()
        {
            var asyncHandle =
                Addressables.LoadAssetAsync<GameObjectSceneReference>(_enemySpawnerGameObjectSceneReferenceAddress);
            
            await asyncHandle.Task;
            if (asyncHandle.Result.Value == null)
            {
                throw new Exception($"Enemy spawner scene instance is not assigned.");
            }

            var enemySpawner = asyncHandle.Result.Value.GetComponent<EnemySpawnManager>();
            if (enemySpawner == null)
            {
                throw new Exception($"Enemy spawner component not found on referenced scene object.");
            }

            return enemySpawner;
        }
        
        #endregion

    }
}