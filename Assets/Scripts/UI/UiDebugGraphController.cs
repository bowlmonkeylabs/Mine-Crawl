using System;
using BML.Script.Intensity;
using BML.ScriptableObjectCore.Scripts.Variables;
using Intensity;
using Private.DebugGUI;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiDebugGraphController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private int _graphGroup = 0;
        [SerializeField] private float _graphUpdatePeriod = 1f;
        [SerializeField] private FloatVariable _playerIntensityScore;
        [SerializeField] private EnemySpawnerParams _currentParams;
        [SerializeField] private IntensityResponseStateData _intensityResponse;
        
        private string _graphCombinedMinMax => $"combinedMinMax{_graphGroup}";
        private string _graphIntensity => $"playerIntensityScore{_graphGroup}";
        private string _graphIntensityTarget => $"playerIntensityScoreTarget{_graphGroup}";

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            _nextUpdateTime = 0;
            DebugGUI.SetGraphProperties(
                _graphIntensity,
                "Intensity",
                0, 1,
                _graphGroup,
                Color.white,
                true,
                false,
                true
            );
            DebugGUI.SetGraphProperties(
                _graphIntensityTarget,
                "Target",
                0, 1,
                _graphGroup,
                Color.cyan,
                true,
                false,
                true
            );
            DebugGUI.SetGraphProperties(
                _graphCombinedMinMax,
                "",
                0, 1,
                _graphGroup,
                Color.white,
                true,
                true,
                false
            );
        }

        private float _nextUpdateTime;
        private void FixedUpdate()
        {
            if (Time.time < _nextUpdateTime)
            {
                return;
            }
            _nextUpdateTime = Time.time + _graphUpdatePeriod;
            
            Color intensityResponseColor;
            switch (_intensityResponse.Value)
            {
                case IntensityController.IntensityResponse.Decreasing:
                    intensityResponseColor = Color.red;
                    break;
                default:
                case IntensityController.IntensityResponse.Increasing:
                    intensityResponseColor = Color.green;
                    break;
                case IntensityController.IntensityResponse.AboveMax:
                    intensityResponseColor = Color.yellow;
                    break;
                case IntensityController.IntensityResponse.BelowMin:
                    intensityResponseColor = Color.blue;
                    break;
            }
            
            UpdateGraphs(intensityResponseColor);
        }

        #endregion
        
        #region Debug graphs

        protected void UpdateGraphs(Color intensityResponseColor)
        {
            if (!Mathf.Approximately(Time.timeScale, 0f))
            {
                DebugGUI.SetGraphColor(_graphIntensity, intensityResponseColor);
                DebugGUI.Graph(_graphIntensity, _playerIntensityScore.Value, true);
                
                DebugGUI.Graph(_graphIntensityTarget, _currentParams.MaxIntensity, true);

                var intensityMinMax = DebugGUI.GetGraphMinMax(_graphIntensity);
                var intensityTargetMinMax = DebugGUI.GetGraphMinMax(_graphIntensityTarget);
                var combinedMinMax = (
                    min: Mathf.Min(intensityMinMax.min, intensityTargetMinMax.min),
                    max: Mathf.Max(intensityMinMax.max, intensityTargetMinMax.max)
                );
                DebugGUI.SetGraphMinMax(_graphIntensity, combinedMinMax.min, combinedMinMax.max);
                DebugGUI.SetGraphMinMax(_graphIntensityTarget, combinedMinMax.min, combinedMinMax.max);
                DebugGUI.SetGraphMinMax(_graphCombinedMinMax, combinedMinMax.min, combinedMinMax.max);
            }
        }
        
        #endregion
        
    }
}