using System;
using BML.Scripts.Utils;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class FootstepController : MonoBehaviour
    {
        [SerializeField] private float _distanceToStep = 1f;
        [SerializeField] private UnityEvent _onFootstep;

        private float distanceMoved;
        private Vector3 lastPos;

        private void OnEnable()
        {
            distanceMoved = 0;
            lastPos = transform.position;
        }

        private void Update()
        {
            distanceMoved += Vector3.Magnitude((transform.position - lastPos));

            if (distanceMoved > _distanceToStep)
            {
                distanceMoved = 0f;
                _onFootstep.Invoke();
            }

            lastPos = transform.position;
        }
    }
}