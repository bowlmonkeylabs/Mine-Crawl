using UnityEngine;

namespace BML.Script.Audio
{
    public class PauseAudioSource : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;

        public void Pause()
        {
            _source.Pause();
        }

        public void Resume()
        {
            _source.Play();
        }
    }
}