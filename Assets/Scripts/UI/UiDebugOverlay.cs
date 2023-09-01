using System;
using BML.Script.Intensity;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using Intensity;
using TMPro;
using UnityEngine;
using Private.DebugGUI;

namespace BML.Scripts.UI
{
    public class UiDebugOverlay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _intensityResponseText;
        [SerializeField] private TransformSceneReference _playerTransformSceneReference;
        [SerializeField] private FloatVariable _levelTimeElapsed;
        [SerializeField] private GameObjectSceneReference _enemySpawnerRef;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private IntVariable _swingDamage;
        [SerializeField] private IntVariable _sweepDamage;
        [SerializeField] private EvaluateCurveVariable _statPickaxeSwingSpeed;
        [SerializeField] private EvaluateCurveVariable _statPickaxeSweepSpeed;
        [SerializeField] private EvaluateCurveVariable _statSwingCritChance;
        [SerializeField] private BoolVariable _playerInCombat;
        [SerializeField] private BoolVariable _anyEnemiesEngaged;
        [SerializeField] private TimerVariable _playerCombatTimer;
        [SerializeField] private FloatVariable _playerIntensityScore;
        [SerializeField] private IntensityResponseStateData _intensityResponse;
        [SerializeField] private TimerVariable _wormSpawnTimer;
        [SerializeField] private TimerVariable _wormMaxStrengthTimer;

        private float _peakIntensityScore = 0;
        private EnemySpawnerParams enemySpawnParams;

        #region Unity lifecycle

        private void OnEnable()
        {
            InitSpawnParams();
            UpdateText();
        }

        private void Update()
        {
            CalculateValues();
            UpdateText();
        }

        #endregion

        protected void InitSpawnParams()
        {
            enemySpawnParams = _enemySpawnerRef.Value.GetComponent<EnemySpawnManager>()?.EnemySpawnerParams;
        }

        protected void UpdateText()
        {
            _text.text = $@"Player pos: {this.FormatVector3(_playerTransformSceneReference.Value.position)}
Level Time: {this.FormatTime(_levelTimeElapsed.Value)}, {Time.timeScale.ToString("0.00")}x
Seed: {SeedManager.Instance.Seed}
Enemy Spawn Params ----------
Spawn delay: {enemySpawnParams.SpawnDelay.ToString("0.00")}
Count: (Current: {_currentEnemyCount.Value}) (Max: {enemySpawnParams.SpawnCap.ToString("0.00")})
Worm: Spawn Timer: {_wormSpawnTimer.RemainingTime} Max Strength Timer: {_wormMaxStrengthTimer.RemainingTime}
Combat ----------
Intensity Score: (Current: {_playerIntensityScore.Value.ToString("0.00")}) (Target: {enemySpawnParams.MaxIntensity.ToString("0.00")})
Player In Combat: {this.FormatBool(_playerInCombat.Value)}
Any Enemies Engaged: {this.FormatBool(_anyEnemiesEngaged.Value)}
Combat Timer: {_playerCombatTimer.RemainingTime}
Swing: (DPS: {(_swingDamage.Value / _statPickaxeSwingSpeed.Value).ToString("0.00")}) (Crit Chance: {_statSwingCritChance.Value * 100f}%)
Sweep: (DPS: {(_sweepDamage.Value / _statPickaxeSweepSpeed.Value).ToString("0.00")})
";

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
            _intensityResponseText.color = intensityResponseColor;
            
            _intensityResponseText.text = $@"{_intensityResponse.Value.ToString()}";
        }

        private void CalculateValues() {
            if(_playerIntensityScore.Value > _peakIntensityScore) {
                _peakIntensityScore = _playerIntensityScore.Value;
            }
        }
        
        #region Format

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

        private string FormatBool(bool value)
        {
            return value ? "T" : "F";
        }
        
        #endregion
        
    }
}