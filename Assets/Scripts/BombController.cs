using System;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;

namespace BML.Scripts
{
    [RequireComponent(typeof(ExplosiveDamager))]
    public class BombController : MonoBehaviour
    {
        #region Inspector

        [TitleGroup("Fuse")]
        
        [SerializeField] private bool _activateOnStart = true;
        
        [SerializeField] private bool _useFuse = true;
        [SerializeField, ShowIf("_useFuse")] private SafeFloatValueReference _fuseTime;
        [SerializeField, ShowIf("_useFuse")] private SafeFloatValueReference _shortFuseTime;

        [TitleGroup("Effects")]
        
        [SerializeField, Required] private ExplosiveDamager _explosiveDamager;
        
        [SerializeField] private bool _destroyOnExplode = true;
        [SerializeField, ShowIf("_destroyOnExplode"), Required] private GameObject _destroyRoot;

        [TitleGroup("Feedback")]
        
        [SerializeField] private MMF_Player _fuseFeedbacks;
        
        [SerializeField] private float _explosionCueOffsetTime = .25f;
        [SerializeField] private MMF_Player _explosionCueFeedbacks;
        
        [SerializeField] private MMF_Player _explodeFeedbacks;
        [SerializeField] private float _explodeFeedbacksScaleMultiplier = 1f;

        #endregion

        private bool _isActive;
        private bool _isDeActivated;
        private float _activateTime;
        private float _currentFuseTime;

        #region Unity lifecycle
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (_explosiveDamager == null) _explosiveDamager = GetComponent<ExplosiveDamager>();
        }

        #endif

        private void Start()
        {
            if (_activateOnStart)
            {
                Activate();
            }
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            // Feedbacks to cue imminent explosion
            if (_useFuse && _activateTime + _currentFuseTime - _explosionCueOffsetTime < Time.time)
            {
                _explosionCueFeedbacks?.PlayFeedbacks();
            }
        }

        private void FixedUpdate()
        {
            if (!_isActive)
            {
                return;
            }

            if (_useFuse && _activateTime + _currentFuseTime < Time.time)
            {
                Explode();
            }
        }

        #endregion

        #region Public interface and logic

        public void Activate()
        {
            if (_isActive || _isDeActivated)
            {
                return;
            }

            Activate(false);
        }

        public void ActivateShortFuse()
        {
            Activate(true);
        }

        private void Activate(bool isShortFuse)
        {
            if (!_useFuse)
            {
                Explode();
                return;
            }
            
            if (!_isActive)
            {
                _fuseFeedbacks.PlayFeedbacks();
            }

            _activateTime = Time.time;
            _isActive = true;
            _currentFuseTime = isShortFuse ? _shortFuseTime.Value : _fuseTime.Value;
        }

        public void Deactivate()
        {
            _isActive = false;
            _isDeActivated = true;
        }

        private void Explode()
        {
            _explosiveDamager.ApplyDamage();

            _isActive = false;

            _fuseFeedbacks.StopFeedbacks();
            _fuseFeedbacks.ResetFeedbacks();

            _explodeFeedbacks.transform.localScale = _explosiveDamager.Radius * 2 * _explodeFeedbacksScaleMultiplier * Vector3.one;
            // TODO Scale the speed of the explosion feedbacks based on the size, to make big explosions feel more massive? prob need to interface with the individual layers of explosion effect to do this right (don't want to slow down physics particles for example, mostly just smoke and fade time)
            _explodeFeedbacks.PlayFeedbacks(this.transform.position);

            if (_destroyOnExplode)
            {
                // TODO need to make sure all feedbacks have time to finish playing, but the bomb mesh should disappear immediately
                LeanTween.value(this.gameObject, 0f, 1f, 0.1f).setOnComplete(() =>
                {
                    Destroy(_destroyRoot);
                });
            }
        }

        // public void SetDamageVariable(IntVariable newDamage)
        // {
        //     _damage.SetVariable(newDamage);
        // }
        //
        // public void SetRadiusVariable(FloatVariable newRadius)
        // {
        //     _explosionRadius.SetVariable(newRadius);
        // }

        #endregion
    }
}