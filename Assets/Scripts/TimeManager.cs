using System.Collections;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;

namespace BML.Scripts
{
    public class TimeManager : MonoBehaviour
    { 
        [SerializeField] private GameEvent _onToggleFreezeTime;
        [SerializeField] private GameEvent _onFreezeTime;
        [SerializeField] private GameEvent _onUnfreezeTime;
        [SerializeField] private GameEvent _onResetTimeScale;
        [SerializeField] private GameEvent _onIncreaseTimeScale;
        [SerializeField] private GameEvent _onDecreaseTimeScale;
        [SerializeField] private GameEvent _onSkipFrame;

        private bool isFrozen;
        private bool skipFrame;
        private float timescaleFactor = 1f;
        private float fpsRefresh = .5f;
        private float fpsRefreshTimer;
        private int fps;
        
        #region Unity Methods

        private void OnEnable()
        {
            _onToggleFreezeTime.Subscribe(ToggleFreezeGame);
            _onFreezeTime.Subscribe(FreezeGame);
            _onUnfreezeTime.Subscribe(UnFreezeGame);
            _onSkipFrame.Subscribe(SkipFrame);
            _onIncreaseTimeScale.Subscribe(IncreaseTimeScale);
            _onDecreaseTimeScale.Subscribe(DecreaseTimeScale);
            _onResetTimeScale.Subscribe(ResetTimeScale);
        }
        
        private void OnDisable()
        {
            _onToggleFreezeTime.Unsubscribe(ToggleFreezeGame);
            _onFreezeTime.Unsubscribe(FreezeGame);
            _onUnfreezeTime.Unsubscribe(UnFreezeGame);
            _onSkipFrame.Unsubscribe(SkipFrame);
            _onIncreaseTimeScale.Unsubscribe(IncreaseTimeScale);
            _onDecreaseTimeScale.Unsubscribe(DecreaseTimeScale);
            _onResetTimeScale.Unsubscribe(ResetTimeScale);
        }

        private void Update()
        {
            if (Time.unscaledTime > fpsRefreshTimer)
            {
                fps = (int) (1f / Time.deltaTime);
                fpsRefreshTimer = Time.unscaledTime + fpsRefresh;
            }
        }

        private void LateUpdate()
        {
            if (isFrozen && !skipFrame) Time.timeScale = 0f;
            skipFrame = false;
        }

        #endregion

        #region Timescale

        private void ResetTimeScale()
        {
            timescaleFactor = 1f;
            if (!isFrozen) Time.timeScale = timescaleFactor;
        }

        private void SkipFrame()
        {
            if (!isFrozen) return;
            
            Time.timeScale = 1f;
            skipFrame = true;
        }

        private void IncreaseTimeScale()
        {
            ChangeTimeScale(10);
        }
        
        private void DecreaseTimeScale()
        {
            ChangeTimeScale(-10);
        }

        private void ChangeTimeScale(float percent)
        {
            timescaleFactor += percent / 100f;

            timescaleFactor = Mathf.Clamp(timescaleFactor, .1f, 10f);
            if (!isFrozen) Time.timeScale = timescaleFactor;
        }

        #endregion

        #region Pause/UnPause

        private void ToggleFreezeGame()
        {
            if (!isFrozen) FreezeGame();
            else UnFreezeGame();
        }

        private void FreezeGame()
        {
            Time.timeScale = 0f;
            isFrozen = true;
            AudioListener.pause = true;
            Debug.Log("Froze Game");
        }

        private void UnFreezeGame()
        {
            Time.timeScale = timescaleFactor;
            isFrozen = false;
            AudioListener.pause = false;
            Debug.Log("UnFroze Game");
        }

        #endregion

        #region Debug
        

        // private void OnGUI()
        // {
        //     if (!DebugManager.Instance.EnableDebugGUI) return;
        //
        //     float pivotX = 10f;
        //     float pivotY = 10f;
        //     float height = 20f;
        //     float verticalMargin = 3f;
        //     float smallButtonWidth = 70f;
        //
        //
        //     GUI.TextArea(new Rect(pivotX, pivotY, 210, 20),
        //         $"Timescale: {timescaleFactor} | FPS: {fps}");
        //
        //     if (GUI.Button(new Rect(pivotX, pivotY + (height + verticalMargin), smallButtonWidth, 20),
        //         $"(J) -10%")) ChangeTimeScale(-10f);
        //     ;
        //     if (GUI.Button(new Rect(pivotX + smallButtonWidth, pivotY + (height + verticalMargin), smallButtonWidth, 20),
        //         $"(K) Reset")) ResetTimeScale();
        //     if (GUI.Button(new Rect(pivotX + smallButtonWidth * 2, pivotY + (height + verticalMargin), smallButtonWidth, 20),
        //         $"(L) +10%")) ChangeTimeScale(10f);
        //
        //     if (GUI.Button(new Rect(pivotX, pivotY + (height + verticalMargin) * 2, 210, height),
        //         "(P) Manual Update: " + ((manualUpdate) ? "<Enabled>" : "<Disabled>"))) ToggleManualUpdate();
        //     
        //     if (GUI.Button(new Rect(pivotX + 210, pivotY + (height + verticalMargin) * 2, smallButtonWidth, height),
        //         "(N) Next")) SkipFrame();
        // }

        #endregion
    }
}