using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Enemy;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class CaveWormSpawner : MonoBehaviour
    {
        [SerializeField, TitleGroup("Spawning")] private TimerVariable _wormSpawnTimer;
        [SerializeField, TitleGroup("Spawning")] private TimerVariable _wormMaxStrengthTimer;
        [SerializeField, TitleGroup("Spawning")] private float _spawnRadius = 100f;
        [SerializeField, TitleGroup("Spawning")] private AnimationCurve _speedCurve;
        [SerializeField, TitleGroup("Spawning")] private AnimationCurve _spawnDelayCurve;
        
        [SerializeField, TitleGroup("Feedback")] private TimerVariable _wormWarningTimer;
        [SerializeField, TitleGroup("Feedback")] private MMF_Player _warningFeedback;
        [SerializeField, TitleGroup("Feedback")] private MMF_Player _wormActivatedFeedback;

        
        [SerializeField, TitleGroup("References")] private GameObject _wormPrefab;
        [SerializeField, TitleGroup("References")] private TransformSceneReference _playerRef;

        [SerializeField, FoldoutGroup("Debug")] private bool _enableDebug;

        private float lastWormSpawnTime = Mathf.NegativeInfinity;
        private float percentToMaxStrength;
        private float currentSpawnDelay;
        private float currentSpeed;
        private GameObject spawnedWorm;

        private void OnEnable()
        {
            _wormSpawnTimer.SubscribeFinished(ActivateWorm);
            _wormWarningTimer.SubscribeFinished(PlayWarningFeedbacks);
            _wormSpawnTimer.StartTimer();
            _wormWarningTimer.StartTimer();
            Debug.Log($"Started Timer: isFinished {_wormSpawnTimer.IsFinished}");
        }

        private void OnDisable()
        {
            _wormSpawnTimer.UnsubscribeFinished(ActivateWorm);
            _wormWarningTimer.UnsubscribeFinished(PlayWarningFeedbacks);
        }

        private void Start()
        {
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
            _wormWarningTimer.UpdateTime();

            if (!_wormSpawnTimer.IsFinished)
                return;

            percentToMaxStrength = _wormMaxStrengthTimer.ElapsedTime / _wormMaxStrengthTimer.Duration;
            currentSpawnDelay = _spawnDelayCurve.Evaluate(percentToMaxStrength);
            currentSpeed = _speedCurve.Evaluate(percentToMaxStrength);
        }

        private void HandleSpawning()
        {
            Debug.Log($"isFinished {_wormSpawnTimer.IsFinished}");
            if (!_wormSpawnTimer.IsFinished)
                return;

            if (lastWormSpawnTime + currentSpawnDelay > Time.time)
                return;

            Vector3 spawnPoint = _playerRef.Value.position + Random.onUnitSphere * _spawnRadius;
            Quaternion facePlayerDir = Quaternion.LookRotation((_playerRef.Value.position - spawnPoint).normalized);
            
            if (spawnedWorm == null)
                spawnedWorm = GameObject.Instantiate(_wormPrefab);

            spawnedWorm.transform.position = spawnPoint;
            spawnedWorm.transform.rotation = facePlayerDir;
            
            WormController wormController = spawnedWorm.GetComponent<WormController>();
            wormController.Respawn(currentSpeed);
            
            if (_enableDebug) Debug.Log("Spawned Worm");
            lastWormSpawnTime = Time.time;
        }

        private void ActivateWorm()
        {
            _wormMaxStrengthTimer.StartTimer();
            _wormActivatedFeedback.PlayFeedbacks();
            if (_enableDebug) Debug.Log("Worm Activated");
        }

        private void PlayWarningFeedbacks()
        {
            _warningFeedback.PlayFeedbacks();
            if (_enableDebug) Debug.Log("Worm is approaching...");
        }
    }
}