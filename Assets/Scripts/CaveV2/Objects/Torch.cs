using System;
using BML.ScriptableObjectCore.Scripts.Events;
using Codice.CM.Common;
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
        [ShowInInspector, ReadOnly, NonSerialized] private bool _isBurntOut;

        [TitleGroup("Level influence")] 
        [SerializeField] private DynamicGameEvent _onTorchPlaced;
        
        #endregion

        #region Unity lifecycle

        private void FixedUpdate()
        {
            if (_isBurntOut) return;
            
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= _timeToLive)
            {
                _isBurntOut = true;
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