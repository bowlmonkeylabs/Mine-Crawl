using System;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiAimReticle : MonoBehaviour
    {
        [SerializeField] private Image _reticleImage;
        [SerializeField] private Color _hoverColor = Color.red;

        private Color originalColor;
        
        private void Awake()
        {
            originalColor = _reticleImage.color;
        }

        public void SetReticleHover(bool isHovering)
        {
            _reticleImage.color = isHovering ? _hoverColor : originalColor;
        }
    }
}