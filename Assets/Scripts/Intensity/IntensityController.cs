using System;
using System.Collections;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts;
using BML.Scripts.CaveV2.Objects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Intensity
{
    public class IntensityController : MonoBehaviour
    {
        #region Inspector

        [TitleGroup("Scene References")] 
        [SerializeField] private TransformSceneReference _playerSceneReference;
        
        [TitleGroup("Intensity Parameters")]
        [SerializeField] private SafeFloatValueReference _intensityScore;
        [SerializeField] private float _intensityScoreUpdatePeriod = 1f;
        [SerializeField] private float _intensityScoreDamageMultiplier = 1f;
        [SerializeField] private float _intensityScoreKillMultiplier = 1f;
        [SerializeField] private Vector2 _intensityScoreKillDistanceMinMax = new Vector2(20, 5);
        [SerializeField] private DynamicGameEvent _onEnemyKilled;
        
        [TitleGroup("Intensity Decay")]
        [SerializeField] private float _intensityScoreDecayRate = 1f / 30f;
        [SerializeField] private BoolReference _anyEnemiesEngaged;
        
        [TitleGroup("Intensity Response")]
        [SerializeField] private SafeFloatValueReference _maxIntensityScore;
        [SerializeField] private SafeFloatValueReference _minIntensityScore;
        [SerializeField] private IntVariable _currentDifficulty;
        [SerializeField] private AnimationCurve _maxIntensityCurve;
        [SerializeField] private float _timeLimitAtMaxIntensity = 5f;
        [SerializeField] private float _timeLimitAtMinIntensity = 5f;

        [TitleGroup("Spawner Parameters")]
        [SerializeField] private BoolVariable _isSpawningPaused;
        [SerializeField] private BoolVariable _hasPlayerExitedStartRoom;
        
        [TitleGroup("Debug")]
        [SerializeField] private bool _enableLogs;

        [ShowInInspector, ReadOnly] private IntensityResponse intensityResponse;

        #endregion

        private Health playerHealthController;
        private float lastUpdateTime = Mathf.NegativeInfinity;
        private float timeAtMaxIntensity, timeAtMinIntensity;

        enum IntensityResponse
        {
            Increasing,
            AboveMax,
            Decreasing,
            BelowMin,
        }

        #region Unity Lifecycle

        private void Awake()
        {
            playerHealthController = _playerSceneReference.Value.GetComponent<Health>();
        }

        private void Start()
        {
            intensityResponse = IntensityResponse.Increasing;
        }

        private void OnEnable()
        {
            _currentDifficulty.Subscribe(UpdateMaxIntensity);
            _onEnemyKilled.Subscribe(OnEnemyKilledDynamic);
            playerHealthController.OnHealthChange += OnPlayerHealthChanged;
        }

        private void OnDisable()
        {
            _currentDifficulty.Unsubscribe(UpdateMaxIntensity);
            _onEnemyKilled.Unsubscribe(OnEnemyKilledDynamic);
            playerHealthController.OnHealthChange -= OnPlayerHealthChanged;
        }

        private void Update()
        {
            if (!_hasPlayerExitedStartRoom.Value)
                return;
            
            if (lastUpdateTime + _intensityScoreUpdatePeriod > Time.time)
                return;

            TryDecayIntensityScore();
            UpdateIntensityResponse();
            
            lastUpdateTime = Time.time;
        }

        #endregion

        #region Update Intensity

        private void UpdateIntensityResponse()
        {
            switch (intensityResponse)
            {
                case IntensityResponse.Increasing:
                    
                    // Just reached max intensity threshold
                    if (_intensityScore.Value >= _maxIntensityScore.Value)
                    {
                        intensityResponse = IntensityResponse.AboveMax;
                    }

                    break;
                
                case IntensityResponse.AboveMax:
                    
                    // Accumulating time at max intensity
                    if (_intensityScore.Value >= _maxIntensityScore.Value)
                    {
                        timeAtMaxIntensity += _intensityScoreUpdatePeriod;
                    }
            
                    // Stop accumulating and reset if drop below threshold
                    if (_intensityScore.Value < _maxIntensityScore.Value)
                    {
                        intensityResponse = IntensityResponse.Increasing;
                        timeAtMaxIntensity = 0f;
                        if (_enableLogs) Debug.Log("Intensity dropped below threshold, resetting...");
                    }
                
                    // If at threshold for enough time, change to Decreasing state
                    if (timeAtMaxIntensity >= _timeLimitAtMaxIntensity)
                    {
                        intensityResponse = IntensityResponse.Decreasing;
                        _isSpawningPaused.Value = true;
                        timeAtMaxIntensity = 0f;
                        if (_enableLogs) Debug.Log("Entering Decreasing Intensity State");
                    }

                    break;
                
                case IntensityResponse.Decreasing:
                    
                    // Just reached min intensity threshold
                    if (_intensityScore.Value <= _minIntensityScore.Value)
                    {
                        intensityResponse = IntensityResponse.BelowMin;
                    }

                    break;
                case IntensityResponse.BelowMin:
                    
                    // Accumulating time at min intensity
                    if (_intensityScore.Value <= _minIntensityScore.Value)
                    {
                        timeAtMinIntensity += _intensityScoreUpdatePeriod;
                    }
            
                    // Stop accumulating and reset if go above threshold
                    if (_intensityScore.Value > _minIntensityScore.Value)
                    {
                        intensityResponse = IntensityResponse.Decreasing;
                        timeAtMinIntensity = 0f;
                        if (_enableLogs) Debug.Log("Intensity went above threshold, resetting...");
                    }
                
                    // If at threshold for enough time, change to Increasing state
                    if (timeAtMinIntensity >= _timeLimitAtMinIntensity)
                    {
                        intensityResponse = IntensityResponse.Increasing;
                        _isSpawningPaused.Value = false;
                        timeAtMinIntensity = 0f;
                        if (_enableLogs) Debug.Log("Entering Increasing Intensity State");
                    }

                    break;
            }
        }

        private void UpdateMaxIntensity()
        {
            _maxIntensityScore.Value = _maxIntensityCurve.Evaluate(_currentDifficulty.Value);
        }

        private void TryDecayIntensityScore()
        {
            bool doDecay = !_anyEnemiesEngaged.Value;

            if (!doDecay) return;

            float decay = (_intensityScoreDecayRate * _intensityScoreUpdatePeriod);
            
            float newIntensityScore = (_intensityScore.Value - decay);
            newIntensityScore = Mathf.Max(0, newIntensityScore);
            _intensityScore.Value = newIntensityScore;
            
            if (_enableLogs)
                Debug.Log($"UpdateIntensityScore (Intensity {_intensityScore.Value})");
        }

        #endregion

        #region Event Callbacks

        private void OnEnemyKilledDynamic(object prev, object curr)
        {
            var payload = curr as EnemyKilledPayload;
            OnEnemyKilled(payload);
        }
        private void OnEnemyKilled(EnemyKilledPayload curr)
        {
            var posDiff = _playerSceneReference.Value.position - curr.Position;
            float dist = posDiff.magnitude;
            float factor = Mathf.InverseLerp(_intensityScoreKillDistanceMinMax.x, _intensityScoreKillDistanceMinMax.y, dist);
            factor = Mathf.Clamp01(factor);
            
            _intensityScore.Value += (factor * _intensityScoreKillMultiplier);
            if (_enableLogs) Debug.Log($"OnEnemyKilled (Intensity {_intensityScore.Value})");
        }
        
        private void OnPlayerHealthChanged(int prev, int curr)
        {
            int delta = curr - prev;
            float deltaPct = Mathf.Abs((float) delta / playerHealthController.StartingHealth);
            if (delta < 0)
            {
                _intensityScore.Value += (deltaPct * _intensityScoreDamageMultiplier);
                if (_enableLogs) Debug.Log($"OnPlayerHealthChanged (Intensity {_intensityScore.Value})");
            }
        }

        #endregion
    }
}