using System;
using System.Collections.Generic;
using BML.Scripts.CaveV2;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class PlayRandomClip : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private CaveGenComponentV2 _caveGenComponent;
        [SerializeField] private List<AudioClip> _clips = new List<AudioClip>();

        private void Start()
        {
            Random.InitState(_caveGenComponent.CaveGenParams.Seed);
            int randomIndex = Random.Range(0, _clips.Count);
            _audioSource.clip = _clips[randomIndex];
            _audioSource.Play();
        }
    }
}