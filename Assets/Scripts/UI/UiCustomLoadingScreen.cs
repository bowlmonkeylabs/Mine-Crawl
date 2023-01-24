using System;
using System.Collections;
using AdvancedSceneManager.Callbacks;
using AdvancedSceneManager.Core;
using AdvancedSceneManager.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiCustomLoadingScreen : LoadingScreen
    {
        #region Inspector

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Min(0f)] private float _fadeOpen = 0f;
        [SerializeField, Min(0f)] private float _fadeClose = 0f;
        [SerializeField] private Slider _sliderProgress;

        #endregion

        #region Unity lifecycle

        private void Update()
        {
            if (operation != null)
            {
                _sliderProgress.value = operation.totalProgress;
            }
        }

        #endregion

        public override IEnumerator OnOpen(SceneOperation operation)
        {
            yield return _canvasGroup.Fade(1, _fadeOpen);
        }

        public override IEnumerator OnClose(SceneOperation operation)
        {
            yield return _canvasGroup.Fade(0, _fadeClose);
        }
        
    }
}