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

        [SerializeField] private FloatVariable _playerIntensityScore;
        [SerializeField] private FloatVariable _playerIntesityTarget;
        [SerializeField] private IntensityResponseStateData _intensityResponse;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            DebugGUI.SetGraphProperties(
                _graphIntensity,
                "Intensity",
                0, 1,
                0,
                Color.white,
                true,
                false,
                true
            );
            DebugGUI.SetGraphProperties(
                _graphIntensityTarget,
                "Target",
                0, 1,
                0,
                Color.cyan,
                true,
                false,
                true
            );
            DebugGUI.SetGraphProperties(
                _graphCombinedMinMax,
                "",
                0, 1,
                0,
                Color.white,
                true,
                true,
                false
            );
        }

        private void Update()
        {
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

        private string _graphCombinedMinMax = "combinedMinMax";
        string _graphIntensity = "playerIntensityScore";
        string _graphIntensityTarget = "playerIntensityScoreTarget";

        protected void UpdateGraphs(Color intensityResponseColor)
        {
            if (!Mathf.Approximately(Time.timeScale, 0f))
            {
                DebugGUI.SetGraphColor(_graphIntensity, intensityResponseColor);
                DebugGUI.Graph(_graphIntensity, _playerIntensityScore.Value, true);
                
                DebugGUI.Graph(_graphIntensityTarget, _playerIntesityTarget.Value, true);

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