using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class PlayRandomClip : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private CaveGenComponentV2 _caveGenComponent;
        [SerializeField] private List<AudioClip> _clips = new List<AudioClip>();

        [ShowInInspector, ReadOnly] private List<AudioClip> randomizedList = new List<AudioClip>();

        private int playingIndex = 0;
        private void Start()
        {
            OrderListRandomly();
            PlayNextClip();
        }

        private void Update()
        {
            if (!_audioSource.isPlaying && Time.timeScale != 0f)
                PlayNextClip();
        }

        private void PlayNextClip()
        {
            _audioSource.clip = randomizedList[playingIndex];
            _audioSource.Play();
            playingIndex++;
            
            // Wrap around
            if (playingIndex > randomizedList.Count - 1)
                playingIndex = 0;
        }

        private void OrderListRandomly()
        {
            randomizedList.Clear();
            Random.InitState(SeedManager.Instance.GetSteppedSeed("RandomClips"));
            randomizedList = _clips.OrderBy(c => Random.value).ToList();
        }
    }
}