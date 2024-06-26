﻿using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Pathfinding;
using QFSW.QC;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("enemy.")]
    public class EnemySpawnerCommands : MonoBehaviour
    {
        [SerializeField] private EnemySpawnManager _enemySpawnManager;
        [SerializeField] private FloatVariable _intensityScore;
        [SerializeField] private FloatVariable _currentPercentToMaxSpawn;
        [SerializeField] private TransformSceneReference _playerTransformSceneReference;

        [SerializeField] private CaveWormSpawner _caveWormSpawner;

        #region Commands

        [Command("get-intensity", "Displays the current measurement of intensity with respect to what's currently happening to the player. This value is reactive to gameplay, not a set value.")]
        private string GetIntensityScore()
        {
            return $"Intensity: {_intensityScore.Value}";
        }
        
        [Command("set-intensity", "Displays the current measurement of intensity with respect to what's currently happening to the player. This value is reactive to gameplay, not a set value.")]
        private string SetIntensityScore(float value)
        {
            _intensityScore.Value = value;
            return "Set Intensity: {_intensityScore.Value} -> {value}";
        }
        
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

        [Command("none")]
        private void None()
        {
            SetSpawningPaused(true);
            DespawnAll();
            DelayWormTimer(10000f);
            SetWormPaused(true);
        }

        [Command("spawn-at-player", "Spawn an enemy at the player's position. Enemies are referenced by name (e.g. Zombie, Slime)")]
        private void Spawn(string enemyName, int count = 1, int health = -1)
        {
            for (int i = 0; i < count; i++)
            {
                var newEnemy = _enemySpawnManager.SpawnEnemyByName(_enemySpawnManager.EnemySpawnerParams, _playerTransformSceneReference.Value.position,
                    enemyName, false, 3);

                if (health >= 0)
                {
                    var healthComponent = newEnemy.GetComponent<Health>();
                    healthComponent.SetHealth(health);
                }
                
            }
        }
        
        #endregion

        #region Enemy

        [Command("freeze", "Freezes all enemies instantly.")]
        private string Freeze(bool freeze = true)
        {
            var aiScripts = FindObjectsOfType<BMLAIPath>();
            foreach (var aiScript in aiScripts)
            {
                aiScript.enabled = !freeze;
                aiScript.maxSpeed = freeze ? 0f : -1f;
            }

            return $"{aiScripts.Length} enemies {(freeze ? "frozed" : "Unfrozed")}";
        }

        #endregion

        #region Worm commands

        [Command("worm-pause", "Pause worm countdown timer.")]
        private void SetWormPaused(bool paused = true)
        {
            _caveWormSpawner.PauseSpawnTimer(paused);
        }

        [Command("worm-delay", "Delay worm countdown timer.")]
        private void DelayWormTimer(float addTimeSeconds)
        {
            _caveWormSpawner.DelaySpawnTimer(addTimeSeconds);
        }

        [Command("worm-delete", "The maker meets its maker.")]
        private void DeleteWorm(bool killHimExtraHard = false)
        {
            _caveWormSpawner.DeleteWorm(killHimExtraHard);
        }

        #endregion
    }
}