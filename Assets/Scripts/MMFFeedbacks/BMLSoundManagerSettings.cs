using Sirenix.OdinInspector;
using UnityEngine;

namespace MMFFeedbacks
{
    [InlineEditor()]
    [CreateAssetMenu(fileName = "BMLSoundManagerSettings", menuName = "BML/MMFFeedbacks/BMLSoundManagerSettings", order = 0)]
    public class BMLSoundManagerSettings : ScriptableObject
    {
        /// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
        [Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
        [Range(0f,1f)]
        public float SpatialBlend;
        /// Sets the Doppler scale for this AudioSource.
        [Tooltip("Sets the Doppler scale for this AudioSource.")]
        [Range(0f,5f)]
        public float DopplerLevel = 1f;
        /// Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.
        [Tooltip("Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.")]
        [Range(0,360)]
        public int Spread = 0;
        /// Sets/Gets how the AudioSource attenuates over distance.
        [Tooltip("Sets/Gets how the AudioSource attenuates over distance.")]
        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
        /// Sets/Gets how the AudioSource attenuates over distance.
        [Tooltip("Sets/Gets how the AudioSource attenuates over distance.")]
        public bool UseCustomRolloffCurve = false;
        [Tooltip("The curve to use for custom volume rolloff if UseCustomRolloffCurve is true")]
        public AnimationCurve CustomRolloffCurve;
        /// Within the Min distance the AudioSource will cease to grow louder in volume.
        [Tooltip("Within the Min distance the AudioSource will cease to grow louder in volume.")]
        public float MinDistance = 1f;
        /// (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.
        [Tooltip("(Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.")]
        public float MaxDistance = 500f;
    }
}