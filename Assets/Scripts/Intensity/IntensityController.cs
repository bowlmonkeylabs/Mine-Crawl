using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Intensity
{
    public class IntensityController : MonoBehaviour
    {
        #region Inspector
        [TitleGroup("Intensity Parameters")]
        [SerializeField] private SafeFloatValueReference _intensityScore;
        [SerializeField] private SafeFloatValueReference _maxIntensityScore;
        [SerializeField] private SafeFloatValueReference _minIntensityScore;
        [SerializeField] private IntVariable _currentDifficulty;
        [SerializeField] private AnimationCurve _maxIntensityCurve;
        [SerializeField] private float _timeLimitAtMaxIntensity = 5f;
        [SerializeField] private float _timeLimitAtMinIntensity = 5f;
        [SerializeField] private float _pollingTime = 1f;
        
        [TitleGroup("Spawner Parameters")]
        [SerializeField] private BoolVariable _isSpawningPaused;
        [SerializeField] private BoolVariable _hasPlayerExitedStartRoom;
        
        [TitleGroup("Debug")]
        [SerializeField] private bool _enableLogs;

        [ShowInInspector, ReadOnly] private IntensityResponse intensityResponse;

        #endregion
        
        private float lastPollTime = Mathf.NegativeInfinity;
        private float timeAtMaxIntensity, timeAtMinIntensity;

        enum IntensityResponse
        {
            Increasing,
            AboveMax,
            Decreasing,
            BelowMin,
        }

        private void Start()
        {
            intensityResponse = IntensityResponse.Increasing;
        }

        private void OnEnable()
        {
            _currentDifficulty.Subscribe(UpdateMaxIntensity);
        }

        private void OnDisable()
        {
            _currentDifficulty.Unsubscribe(UpdateMaxIntensity);
        }

        private void Update()
        {
            if (!_hasPlayerExitedStartRoom.Value)
                return;
            
            if (lastPollTime + _pollingTime > Time.time)
                return;

            HandleIntensityResponse();
            lastPollTime = Time.time;
        }

        private void HandleIntensityResponse()
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
                        timeAtMaxIntensity += _pollingTime;
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
                        timeAtMinIntensity += _pollingTime;
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
    }
}