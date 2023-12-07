using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2;
using BML.Scripts.Enemy;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class CaveWormSpawner : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField, TitleGroup("Spawning")] private TimerVariable _wormSpawnTimer;
        [SerializeField, TitleGroup("Spawning")] private BoolVariable _playerHasExitedStartRoom;
        [SerializeField, TitleGroup("Spawning")] private TimerVariable _wormMaxStrengthTimer;
        [SerializeField, TitleGroup("Spawning")] private float _spawnRadius = 100f;
        [SerializeField, TitleGroup("Spawning")] private AnimationCurve _speedCurve;
        [SerializeField, TitleGroup("Spawning")] private AnimationCurve _spawnDelayCurve;

        [SerializeField, TitleGroup("Feedback")] private List<WormWarningData> _warningData;
        [SerializeField, TitleGroup("Feedback")] private MMF_Player _wormActivatedFeedback;

        [SerializeField, TitleGroup("References")] private GameObject _wormPrefab;
        [SerializeField, TitleGroup("References")] private TransformSceneReference _mainCamera;

        [SerializeField, FoldoutGroup("Debug")] private bool _enableDebug;

        private float lastWormSpawnTime = Mathf.NegativeInfinity;
        private float percentToMaxStrength;
        private float currentSpawnDelay;
        private float currentSpeed;
        private GameObject spawnedWorm;
        private bool isFirstAttack = true;

        private List<WormBait.WormBait> wormBaits = new List<WormBait.WormBait>();

        // Debug
        private bool killHimAndDontComeBack;

        [System.Serializable]
        public class WormWarningData
        {
            [HideInInspector] public bool Activated;
            public float TimeOffsetFromSpawn;
            public MMF_Player Feedback;
        }
        
        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _wormSpawnTimer.ResetTimer();
            _wormSpawnTimer.SubscribeFinished(ActivateWorm);
            _playerHasExitedStartRoom.Subscribe(StartWormTimer);
            wormBaits.Clear();
        }

        private void OnDisable()
        {
            _wormSpawnTimer.UnsubscribeFinished(ActivateWorm);
            _playerHasExitedStartRoom.Unsubscribe(StartWormTimer);
            Destroy(spawnedWorm);
        }

        private void Update()
        {
            UpdateParameters();
            HandleWarnings();
            HandleSpawning();
        }

        #endregion

        #region Inner wormkings

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

        private void HandleWarnings()
        {
            foreach (var warning in _warningData)
            {
                if (!warning.Activated && _wormSpawnTimer.RemainingTime < warning.TimeOffsetFromSpawn)
                {
                    warning.Feedback.PlayFeedbacks();
                    warning.Activated = true;
                    if (_enableDebug) Debug.Log($"Warning with offset {warning.TimeOffsetFromSpawn} Played");
                }
            }
        }

        private void HandleSpawning()
        {
            if (killHimAndDontComeBack || !_wormSpawnTimer.IsFinished)
                return;

            if (lastWormSpawnTime + currentSpawnDelay > Time.time)
                return;

            Vector3 playerForwardFlat = _mainCamera.Value.forward.xoz().normalized;
            Vector3 spawnDir;

            if (isFirstAttack)
            {
                spawnDir = playerForwardFlat;
                isFirstAttack = false;
            }
            else
            {
                Random.InitState(SeedManager.Instance.GetSteppedSeed("WormSpawner"));
                spawnDir = Random.value > .5f ? playerForwardFlat : Vector3.up;
            }
            
            Vector3 spawnPoint, targetPoint;
            if (wormBaits.Count > 0)
            {
                targetPoint = wormBaits[0].transform.position;
                UnRegisterWormBait(wormBaits[0]);
                spawnDir = Vector3.up;
            }
            else
            {
                targetPoint = _mainCamera.Value.position;
            }

            spawnPoint = targetPoint + spawnDir * _spawnRadius;
            Quaternion wormFaceDir = Quaternion.LookRotation(-spawnDir.normalized);
            
            if (spawnedWorm == null)
                spawnedWorm = GameObject.Instantiate(_wormPrefab);

            spawnedWorm.transform.position = spawnPoint;
            spawnedWorm.transform.rotation = wormFaceDir;
            
            WormController wormController = spawnedWorm.GetComponent<WormController>();
            wormController.Respawn(currentSpeed, targetPoint);
            
            lastWormSpawnTime = Time.time;
            SeedManager.Instance.UpdateSteppedSeed("WormSpawner");
        }

        private void ActivateWorm()
        {
            _wormMaxStrengthTimer.RestartTimer();
            _wormActivatedFeedback.PlayFeedbacks();
            if (_enableDebug) Debug.Log("Worm Activated");
        }

        private void StartWormTimer(bool dummy, bool hasExitedStartRoom)
        {
            if (hasExitedStartRoom)
            {
                _wormSpawnTimer.RestartTimer();
                if (_enableDebug) Debug.Log("Starting Worm Spawn Timer");
            }
        }

        #endregion

        #region Public interface

        public void PauseSpawnTimer(bool pause = true)
        {
            if (pause)
            {
                _wormSpawnTimer.StopTimer();
            }
            else
            {
                _wormSpawnTimer.StartTimer();
            }
        }

        public void DelaySpawnTimer(float addSeconds)
        {
            _wormSpawnTimer.AddTime(addSeconds);
        }

        public void DeleteWorm(bool killHimExtraHard = false)
        {
            if (killHimExtraHard)
            {
                killHimAndDontComeBack = true;
            }
            if (spawnedWorm != null)
            {
                Destroy(spawnedWorm);
            }
        }

        public void RegisterWormBait(WormBait.WormBait wormBait)
        {
            wormBaits.Add(wormBait);
        }
        
        public void UnRegisterWormBait(WormBait.WormBait wormBait)
        {
            wormBaits.Remove(wormBait);
        }
        #endregion
        
    }
}