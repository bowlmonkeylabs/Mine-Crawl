using BML.Script.Intensity;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using Intensity;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiDebugOverlay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _intensityResponseText;
        [SerializeField] private TransformSceneReference _playerTransformSceneReference;
        [SerializeField] private FloatVariable _levelTimeElapsed;
        [SerializeField] private FloatVariable _currentSpawnDelay;
        [SerializeField] private FloatVariable _currentSpawnCap;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private IntVariable _currentDifficulty;
        [SerializeField] private IntReference _seedDebugReference;
        [SerializeField] private EvaluateCurveVariable _statPickaxeSwingSpeed;
        [SerializeField] private BoolVariable _playerInCombat;
        [SerializeField] private BoolVariable _anyEnemiesEngaged;
        [SerializeField] private TimerVariable _playerCombatTimer;
        [SerializeField] private FloatVariable _playerIntensityScore;
        [SerializeField] private IntensityResponseStateData _intensityResponse;

        #region Unity lifecycle

        private void Awake()
        {
            UpdateText();
        }

        private void Update()
        {
            UpdateText();
        }

        #endregion
        
        protected void UpdateText()
        {
            _text.text = $@"Player Coordinates: {this.FormatVector3(_playerTransformSceneReference.Value.position)}
Timescale: {Time.timeScale.ToString("0.00")}
Level Time: {this.FormatTime(_levelTimeElapsed.Value)}
Enemy Spawn Params:
Delay: {_currentSpawnDelay.Value.ToString("0.00")}
Cap: {_currentSpawnCap.Value.ToString("0.00")}
Count: {_currentEnemyCount.Value}
Difficulty: {_currentDifficulty.Value}
Seed: {_seedDebugReference.Value}
Pickaxe Swing Speed: {_statPickaxeSwingSpeed.Value}
Player In Combat: {_playerInCombat.Value}
Any Enemies Engaged: {_anyEnemiesEngaged.Value}
Combat Timer: {_playerCombatTimer.RemainingTime}
Intensity Score: {_playerIntensityScore.Value.ToString("0.00")}
";

            switch (_intensityResponse.Value)
            {
                case IntensityController.IntensityResponse.Decreasing:
                    _intensityResponseText.color = Color.red;
                    break;
                case IntensityController.IntensityResponse.Increasing:
                    _intensityResponseText.color = Color.green;
                    break;
                case IntensityController.IntensityResponse.AboveMax:
                    _intensityResponseText.color = Color.yellow;
                    break;
                case IntensityController.IntensityResponse.BelowMin:
                    _intensityResponseText.color = Color.blue;
                    break;
            }
            
            _intensityResponseText.text = $@"{_intensityResponse.Value.ToString()}";
        }

        private string FormatVector3(Vector3 vector3) 
        {
            return $"{vector3.x.ToString("0.00")}, {vector3.y.ToString("0.00")}, {vector3.z.ToString("0.00")}";
        }
        
        private string FormatTime(float seconds)
        {
            int minutes = (int) seconds / 60;
            seconds %= 60;
            int hours = (int) minutes / 60;
            minutes %= 60;
            return $"{hours.ToString("00")}:{minutes.ToString("00")}:{seconds.ToString("00")}";
        }
    }
}