using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiCustomLoadingScreen : MonoBehaviour
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
 
        }

        #endregion

        
    }
}