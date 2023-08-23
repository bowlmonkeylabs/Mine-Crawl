using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private GameEvent _wormWarningEvent;
        [SerializeField] private TimerVariable _wormSpawnTimer;
        [SerializeField] private AudioClip _wormWarningMusic;
        [SerializeField] private AudioClip _wormSpawnMusic;
        [SerializeField] private PlayRandomClip _levelMusicPlayer;
        [SerializeField] private AudioSource _audioSource;

        private void OnEnable()
        {
            _wormWarningEvent.Subscribe(PlayWormWarningMusic);
            _wormSpawnTimer.SubscribeFinished(PlayWormSpawnMusic);
        }

        private void OnDisable()
        {
            _wormWarningEvent.Unsubscribe(PlayWormWarningMusic);
            _wormSpawnTimer.UnsubscribeFinished(PlayWormSpawnMusic);
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