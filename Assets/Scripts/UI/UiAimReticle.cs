using System;
using ThisOtherThing.UI.Shapes;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiAimReticle : MonoBehaviour
    {
        [SerializeField] private RectTransform _uiRoot;
        [SerializeField] private Image _reticleImage;
        [SerializeField] private Color _hoverColor = Color.red;

        private Color originalColor;
        
        private void Awake()
        {
            originalColor = _reticleImage.color;
        }

        public void SetReticleHover(bool isHovering)
        {
            var color = isHovering ? _hoverColor : originalColor;
            _reticleImage.color = color;
            // _swingTimerArc.ShapeProperties.FillColor = color;
            // _swingTimerArc.ForceMeshUpdate();
            // _sweepTimerArc.ShapeProperties.FillColor = color;
            // _sweepTimerArc.ForceMeshUpdate();
        }

        public void SetReticleScaling(float scale)
        {
            _uiRoot.localScale = Vector3.one * scale;
        }
        
    }
}