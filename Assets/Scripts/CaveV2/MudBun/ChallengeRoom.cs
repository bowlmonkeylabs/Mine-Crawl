using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts.CaveV2.MudBun
{
    public class ChallengeRoom : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onChallengeStartedEvent;

        public event EventHandler _onChallengeStarted;

        public void StartChallenge() {
            _onChallengeStartedEvent.Invoke();
            _onChallengeStarted?.Invoke(this, new EventArgs());
        }
    }
}
