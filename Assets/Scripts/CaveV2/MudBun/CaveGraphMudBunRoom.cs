using System;
using System.Collections.Generic;
using System.ComponentModel;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.MudBun
{
    public class CaveGraphMudBunRoom : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private GameObject _connectionPortPrefab;
        [SerializeField] private float _connectionPortBoundsRadius = 10;
        [ShowInInspector] private List<CaveNodeConnectionPort> _connectionPorts;
        public List<CaveNodeConnectionPort> ConnectionPorts { get{ return _connectionPorts; } }

        [Button]
        private void AddConnectionPort() {
            _connectionPortPrefab.GetComponent<CaveNodeConnectionPort>()._caveGraphMudBunRoom = this;
            var connectionPort = GameObjectUtils.SafeInstantiate(true, _connectionPortPrefab, this.transform);
            _connectionPorts.Add(connectionPort.GetComponent<CaveNodeConnectionPort>());
        }

        #endregion

        public float ConnectionPortBoundsRadius { get { return _connectionPortBoundsRadius; } }
 
        #region Unity lifecycle

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (_connectionPorts == null)
            {
                _connectionPorts = new List<CaveNodeConnectionPort>();
            }
            else
            {
                _connectionPorts.Clear();
            }
            _connectionPorts.AddRange(GetComponentsInChildren<CaveNodeConnectionPort>());
#endif
        }

        void OnDrawGizmosSelected() {
#if UNITY_EDITOR
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, _connectionPortBoundsRadius);
#endif
        }

        #endregion
        
    }
}