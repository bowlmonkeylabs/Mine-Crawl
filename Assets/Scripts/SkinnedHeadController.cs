using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class SkinnedHeadController : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] [MinMaxSlider(-1f, 1f)] private Vector2 _clipLoudnessMinMax;
        public float updateStep = 0.1f;
        public int sampleDataLength = 1024;
 
        private float currentUpdateTime = 0f;
 
        private float clipLoudness;
        private float[] clipSampleData;
 
        void Awake ()
        {
            clipSampleData = new float[sampleDataLength];
        }
     
        void Update ()
        {
            currentUpdateTime += Time.deltaTime;
            if (currentUpdateTime >= updateStep) {
                currentUpdateTime = 0f;
                
                SampleAudioSourceVolume();
                
                UpdateBlendShape();
            }
 
        }

        private void SampleAudioSourceVolume()
        {
            _audioSource.clip.GetData(clipSampleData, _audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
            clipLoudness = 0f;
            foreach (var sample in clipSampleData) {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for
        }

        private void UpdateBlendShape()
        {
            var pct = Mathf.InverseLerp(_clipLoudnessMinMax.x, _clipLoudnessMinMax.y, clipLoudness);
            var weight = Mathf.Lerp(0f, 100f, pct);
            
            _skinnedMeshRenderer.SetBlendShapeWeight(0, weight);
        }
    }
}