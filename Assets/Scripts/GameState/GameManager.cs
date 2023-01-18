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

        [SerializeField] private BoolVariable _isGameLost;
        [SerializeField] private FloatVariable _levelStartTime;
        [SerializeField] private FloatVariable _levelElapsedTime;

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
            _restartFeedbacks.PlayFeedbacks();
        }

        public void QuitGame()
        {
            _quitFeedbacks.PlayFeedbacks();
        }
        
        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            _restartGame.Subscribe(RestartGame);
            _quitGame.Subscribe(QuitGame);
        }

        private void OnDisable()
        {
            _restartGame.Unsubscribe(RestartGame);
            _quitGame.Unsubscribe(QuitGame);
        }

        private void FixedUpdate()
        {
            _levelElapsedTime.Value = Time.time - _levelStartTime.Value;
        }

        #endregion

        public void DebugLog(string message)
        {
            Debug.Log(message);
        }
    }
}