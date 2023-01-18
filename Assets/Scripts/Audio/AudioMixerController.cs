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
        [SerializeField] private FloatReference _settingMasterVolume;
        [SerializeField] private FloatReference _settingMusicVolume;
        [SerializeField] private FloatReference _settingSfxVolume;

        private void Start()
        {
            SetMasterVolume(0f, _settingMasterVolume.Value);
            SetMusicVolume(0f, _settingMusicVolume.Value);
            SetSfxVolume(0f, _settingSfxVolume.Value);
        }

        private void OnEnable()
        {
            SetMasterVolume(0f, _settingMasterVolume.Value);
            SetMusicVolume(0f, _settingMusicVolume.Value);
            SetSfxVolume(0f, _settingSfxVolume.Value);
            _settingMasterVolume.Subscribe(SetMasterVolume);
            _settingMusicVolume.Subscribe(SetMusicVolume);
            _settingSfxVolume.Subscribe(SetSfxVolume);
        }
        
        private void OnDisable()
        {
            _settingMasterVolume.Unsubscribe(SetMasterVolume);
            _settingMusicVolume.Unsubscribe(SetMusicVolume);
            _settingSfxVolume.Unsubscribe(SetSfxVolume);
        }

        private void SetMasterVolume(float previousVolume, float newVolume)
        {
            float volume = ProcessVolume(newVolume);
            _audioMixer.SetFloat("MasterVolume", volume);
        }

        private void SetMusicVolume(float previousVolume, float newVolume)
        {
            float volume = ProcessVolume(newVolume);
            _audioMixer.SetFloat("MusicVolume", volume);
        }

        private void SetSfxVolume(float previousVolume, float newVolume)
        {
            float volume = ProcessVolume(newVolume);
            _audioMixer.SetFloat("SfxVolume", volume);
        }

        private float ProcessVolume(float volume)
        {
            float processedVolume;
            
            if (Mathf.Approximately(volume, 0f))
                processedVolume = -80f;
            else
                processedVolume = Mathf.Log10(volume) * 20f;

            return processedVolume;
        }
    }
}