using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace BML.Scripts
{
    public class AudioMixerController : MonoBehaviour
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private FloatReference _settingVolume;

        private void Start()
        {
            SetVolume( _settingVolume.Value);
        }

        private void OnEnable()
        {
            SetVolume( _settingVolume.Value);
            _settingVolume.Subscribe(SetVolume);
        }
        
        private void OnDisable()
        {
            _settingVolume.Unsubscribe(SetVolume);
        }

        private void SetVolume(float newVolume)
        {
            SetVolume(0f, newVolume);
        }

        private void SetVolume(float previousVolume, float newVolume)
        {
            float volume;

            if (Mathf.Approximately(newVolume, 0f))
                volume = -80f;
            else
                volume = Mathf.Log10(newVolume) * 20f;
            
            _audioMixer.SetFloat("MasterVolume", volume);
        }
    }
}