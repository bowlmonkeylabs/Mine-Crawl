using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Enemy;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class CaveWormSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _wormPrefab;
        [SerializeField] private TimerVariable _wormSpawnTimer;
        [SerializeField] private TimerVariable _wormMaxStrengthTimer;
        [SerializeField] private TransformSceneReference _playerRef;
        [SerializeField] private float _spawnRadius = 100f;
        [SerializeField] private AnimationCurve _speedCurve;
        [SerializeField] private AnimationCurve _spawnDelayCurve;

        [SerializeField] [FoldoutGroup("Debug")] private bool _enableDebug;

        private float lastWormSpawnTime = Mathf.NegativeInfinity;
        private float percentToMaxStrength;
        private float currentSpawnDelay;
        private float currentSpeed;
        private GameObject spawnedWorm;

        private void OnEnable()
        {
            _wormSpawnTimer.SubscribeFinished(ActivateWorm);
        }

        private void OnDisable()
        {
            _wormSpawnTimer.UnsubscribeFinished(ActivateWorm);
        }

        private void Start()
        {
            _wormSpawnTimer.StartTimer();
            if (_enableDebug) Debug.Log("Starting Worm Spawn Timer");
        }

        private void Update()
        {
            UpdateParameters();
            HandleSpawning();
        }

        private void UpdateParameters()
        {
            _wormSpawnTimer.UpdateTime();
            _wormMaxStrengthTimer.UpdateTime();

            if (!_wormSpawnTimer.IsFinished)
                return;

            percentToMaxStrength = _wormMaxStrengthTimer.ElapsedTime / _wormMaxStrengthTimer.Duration;
            currentSpawnDelay = _spawnDelayCurve.Evaluate(percentToMaxStrength);
            currentSpeed = _speedCurve.Evaluate(percentToMaxStrength);
        }

        private void HandleSpawning()
        {
            if (!_wormSpawnTimer.IsFinished)
                return;

            if (lastWormSpawnTime + currentSpawnDelay > Time.time)
                return;
            
            if (spawnedWorm != null)
                GameObject.Destroy(spawnedWorm);

            Vector3 spawnPoint = _playerRef.Value.position + Random.onUnitSphere * _spawnRadius;
            Quaternion facePlayerDir = Quaternion.LookRotation((_playerRef.Value.position - spawnPoint).normalized);
            spawnedWorm = GameObject.Instantiate(_wormPrefab, spawnPoint, facePlayerDir);
            
            WormController wormController = spawnedWorm.GetComponent<WormController>();
            wormController.SetSpeed(currentSpeed);
            
            if (_enableDebug) Debug.Log("Spawned Worm");
            lastWormSpawnTime = Time.time;
        }

        private void ActivateWorm()
        {
            _wormMaxStrengthTimer.StartTimer();
            if (_enableDebug) Debug.Log("Worm Activated");
        }
    }
}