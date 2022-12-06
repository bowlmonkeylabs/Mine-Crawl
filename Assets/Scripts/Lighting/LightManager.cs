using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Lighting
{
    public class LightManager : MonoBehaviour
    {
        [SerializeField] private FloatVariable _gammaSetting;
        
        private PostProcessProfile postProcessProfile;
        
        #region Unity lifecycle

        private void OnEnable()
        {
            postProcessProfile = GetComponent<PostProcessVolume>().profile;
            SetGammaLevel();
            _gammaSetting.Subscribe(SetGammaLevel);
        }
        
        private void OnDisable()
        {
            _gammaSetting.Unsubscribe(SetGammaLevel);
        }

        #endregion
        
        public void SetGammaLevel()
        {
            
            var ColorGrading = postProcessProfile.GetSetting<ColorGrading>();
            if (ColorGrading == null)
            {
                Debug.LogWarning("Color Grading effect missing from PostProcessing! Not able to adjust gamma.");
                return;
            }

            ColorGrading.gamma.value = new Vector4(1f, 1f, 1f, _gammaSetting.Value);
        }
    }
}