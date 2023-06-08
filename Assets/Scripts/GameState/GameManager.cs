
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameEvent _restartGame;
        [SerializeField] private GameEvent _quitGame;
        [SerializeField] private GameEvent _loadNextLevel;
        [SerializeField] private GameEvent _levelChange;


        [SerializeField] private BoolVariable _isGameLost;
        [SerializeField] private FloatVariable _levelStartTime;
        [SerializeField] private FloatVariable _levelElapsedTime;

        [SerializeField] private IntVariable _currentLevel;
        [SerializeField] private UnityEvent _onAllLevelsCompleted;
        [SerializeField] private LevelSceneCollections _levels;

        private void Awake() {

        }

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
        }

        public void QuitGame()
        {
        }

        public void LoadNextLevel() {

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