using System;
using System.Collections.Generic;
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

        [SerializeField] private MMF_Player _restartFeedbacks;
        [SerializeField] private MMF_Player _quitFeedbacks;

        [TitleGroup("Reset Variables")]
        [SerializeField] private BoolVariable _isPaused;
        [SerializeField] private BoolVariable _isGameLost;
        [SerializeField] private BoolVariable _isGameWon;
        [SerializeField] private BoolVariable _isMenuOpenStore;
        [SerializeField] private IntVariable _playerHealth;
        [SerializeField] private TimerVariable _levelTimer;

        private void Awake()
        {
            ResetVariables();
        }

        private void Start()
        {
            _levelTimer.StartTimer();
        }

        private void ResetVariables()
        {
            _isPaused.Reset();
            _isGameLost.Reset();
            _isGameWon.Reset();
            _isMenuOpenStore.Reset();
            _playerHealth.Reset();
            _levelTimer.ResetTimer();
        }
        
        #region Public interface

        public void LoseGame()
        {
            _isGameLost.Value = true;
        }
        
        public void RestartGame()
        {
            ResetVariables();
            _restartFeedbacks.PlayFeedbacks();
        }

        public void QuitGame()
        {
            ResetVariables();
            _quitFeedbacks.PlayFeedbacks();
        }
        
        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            _levelTimer.SubscribeFinished(LoseGame);
            _restartGame.Subscribe(RestartGame);
            _quitGame.Subscribe(QuitGame);
        }

        private void OnDisable()
        {
            _levelTimer.UnsubscribeFinished(LoseGame);
            _restartGame.Unsubscribe(RestartGame);
            _quitGame.Unsubscribe(QuitGame);
        }

        private void FixedUpdate()
        {
            _levelTimer.UpdateTime();
        }

        #endregion

        public void DebugLog(string message)
        {
            Debug.Log(message);
        }
    }
}