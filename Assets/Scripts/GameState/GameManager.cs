using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Utility;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameEvent _restartGame;
        [SerializeField] private GameEvent _quitGame;
        [SerializeField] private GameEvent _loadNextLevel;
        [SerializeField] private GameEvent _levelChange;

        [SerializeField] private SceneCollection _quitSceneCollection;

        [SerializeField] private BoolVariable _isGameLost;
        [SerializeField] private FloatVariable _levelStartTime;
        [SerializeField] private FloatVariable _levelElapsedTime;

        [SerializeField] private IntVariable _currentLevel;
        [SerializeField] private UnityEvent _onAllLevelsCompleted;
        [SerializeField] private LevelSceneCollections _levels;

        private void Start()
        {
            _levelStartTime.Value = Time.time;
        }

        #region Public interface

        public void LoseGame()
        {
            _isGameLost.Value = true;
        }
        
        public void RestartGame()
        {
            _levels.Levels[_currentLevel.Value].Close();
            _currentLevel.Reset();
            _levels.Levels[_currentLevel.Value].Open();
        }

        public void QuitGame()
        {
            _quitSceneCollection.OpenOrReopen();
        }

        public void LoadNextLevel() {
            int nextLevelIndex = _currentLevel.Value + 1;
            if(nextLevelIndex < _levels.Levels.Count) {
                _currentLevel.Value = nextLevelIndex;
                _levels.Levels[nextLevelIndex].Open();
                _levelChange.Raise();
            } else {
                _onAllLevelsCompleted.Invoke();
            }
        }
        
        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            _restartGame.Subscribe(RestartGame);
            _quitGame.Subscribe(QuitGame);
            _loadNextLevel.Subscribe(LoadNextLevel);
        }

        private void OnDisable()
        {
            _restartGame.Unsubscribe(RestartGame);
            _quitGame.Unsubscribe(QuitGame);
            _loadNextLevel.Unsubscribe(LoadNextLevel);
        }

        private void FixedUpdate()
        {
            _levelElapsedTime.Value = Time.time - _levelStartTime.Value;
        }

        #endregion
    }
}