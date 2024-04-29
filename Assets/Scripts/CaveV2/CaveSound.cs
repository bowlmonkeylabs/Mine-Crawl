using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    [RequireComponent(typeof(AudioSource))]
    public class CaveSound : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;
        
        // continuously update position based on "cave sound" augment
        [SerializeField, Required] private AudioSource _audioSource;

        [SerializeField, Required, LabelText("Cave Generator")] 
        private GameObjectSceneReference _caveGenComponentGameObjectSceneReference;
        private CaveGenComponentV2 _caveGenerator => _caveGenComponentGameObjectSceneReference.CachedComponent as CaveGenComponentV2;
        
        [SerializeField] private GameEvent _onAfterUpdatePlayerDistance;

        private float _initialVolume;
        private AnimationCurve _initialCustomRolloffCurve;
        private float _initialMaxDistance;

        #region Unity lifecycle

        private void OnValidate()
        {
            if (_anchor != null && _anchor.position != transform.position)
            {
                transform.position = _anchor.position;
            }
        }

        private void Start()
        {
            _initialVolume = _audioSource.volume;
            _initialCustomRolloffCurve = _audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
            _initialMaxDistance = _audioSource.maxDistance;
        }

        private void OnEnable()
        {
            _onAfterUpdatePlayerDistance.Subscribe(UpdateCaveSound);
        }
        
        private void OnDisable()
        {
            _onAfterUpdatePlayerDistance.Unsubscribe(UpdateCaveSound);
        }

        private void FixedUpdate()
        {
            if (_audioSource.isPlaying)
            {
                // UpdateCaveSound();
                // TODO interpolate based on player position
            }
        }

        #endregion

        private void UpdateCaveSound()
        {
            // Debug.Log($"updating cave sound ({this.gameObject.name})");
            
            var (localPosition, volume, rolloffCurve, maxDistance) = _caveGenerator.CaveGraph.AugmentAsCaveSound(_anchor.position, _initialVolume, _initialCustomRolloffCurve, _initialMaxDistance);
            var newWorldPosition = _caveGenerator.LocalToWorld(localPosition);
            
            transform.position = newWorldPosition;
            _audioSource.volume = volume;
            _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloffCurve);
            _audioSource.maxDistance = maxDistance;
        }
        
    }
}