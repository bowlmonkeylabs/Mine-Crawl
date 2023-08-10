using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.MudBun
{
    public class CaveGraphMudBunRoom : MonoBehaviour
    {
        #region Inspector

        [ShowInInspector] private List<CaveNodeConnectionPort> _connectionPorts;
        public List<CaveNodeConnectionPort> ConnectionPorts { get{ return _connectionPorts; } }

        #endregion

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

        #endregion
        
    }
}