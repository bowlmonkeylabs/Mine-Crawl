using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;

namespace Lighting
{
    public class LightManager : MonoBehaviour
    {
        [SerializeField] private FloatVariable _brightnessSetting;
        
        #region Unity lifecycle

        private void OnEnable()
        {
            SetAmbientLight();
            _brightnessSetting.Subscribe(SetAmbientLight);
        }
        
        private void OnDisable()
        {
            _brightnessSetting.Unsubscribe(SetAmbientLight);
        }

        #endregion
        
        public void SetAmbientLight()
        {
            float c = _brightnessSetting.Value / 255f;
            RenderSettings.ambientLight = new Color(c, c, c, 1f);
        }
    }
}