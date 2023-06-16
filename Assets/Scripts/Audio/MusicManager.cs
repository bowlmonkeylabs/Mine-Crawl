using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private TimerVariable _wormWarningTimer;
        [SerializeField] private TimerVariable _wormSpawnTimer;
        [SerializeField] private AudioClip _wormWarningMusic;
        [SerializeField] private AudioClip _wormSpawnMusic;
        [SerializeField] private PlayRandomClip _levelMusicPlayer;
        [SerializeField] private AudioSource _audioSource;

        private void Awake()
        {
            _wormWarningTimer.SubscribeFinished(PlayWormWarningMusic);
            _wormSpawnTimer.SubscribeFinished(PlayWormSpawnMusic);
        }

        private void PlayWormWarningMusic()
        {
            if (_levelMusicPlayer.enabled)
                _levelMusicPlayer.enabled = false;
            
            if (_audioSource.clip == _wormWarningMusic)
                return;

            _audioSource.clip = _wormWarningMusic;
            _audioSource.Play();
        }
        
        private void PlayWormSpawnMusic()
        {
            if (_levelMusicPlayer.enabled)
                _levelMusicPlayer.enabled = false;
            
            if (_audioSource.clip == _wormSpawnMusic)
                return;

            _audioSource.clip = _wormSpawnMusic;
            _audioSource.Play();
        }
    }
}