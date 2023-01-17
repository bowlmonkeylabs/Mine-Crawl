using System;
using BML.ScriptableObjectCore.Scripts.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.Objects
{
    public class Torch : MonoBehaviour
    {
        #region Inspector

        [TitleGroup("Burn out")]
        [SerializeField, SuffixLabel("seconds")] private float _timeToLive = 120f;
        [ShowInInspector, ReadOnly, NonSerialized] private float _elapsedTime;
        [ShowInInspector, ReadOnly] public float PercentRemaining => 1f - (_elapsedTime / _timeToLive);
        [ShowInInspector, ReadOnly] public bool IsBurntOut { get; private set; }

        [TitleGroup("Level influence")] 
        [SerializeField] private DynamicGameEvent _onTorchPlaced;
        
        #endregion

        #region Unity lifecycle

        private void FixedUpdate()
        {
            if (IsBurntOut) return;
            
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= _timeToLive)
            {
                IsBurntOut = true;
            }
        }

        #endregion

        #region Public interface

        public void OnTorchPlaced()
        {
            var payload = new TorchPlacedPayload
            {
                Position = this.transform.position,
                Torch = this,
            };
            _onTorchPlaced.Raise(payload);
        }

        #endregion
    }
}