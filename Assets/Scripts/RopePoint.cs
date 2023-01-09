using System.Collections;
using BML.ScriptableObjectCore.Scripts.Variables;
using MoreMountains.Tools;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Events;

namespace BML.Scripts
{
    public class RopePoint : MonoBehaviour
    {
        [SerializeField] private Collider _trigger;
        [SerializeField] private string _playerTag;
        [SerializeField] private LayerMask _playerLayerMask;
        [SerializeField] private BoolReference _isRopeMovementEnabled;
        [SerializeField] private DynamicGameEvent _playerRopePointStateChanged;

        void OnTriggerEnter(Collider collider) {
            if(_isRopeMovementEnabled.Value && _playerLayerMask.MMContains(collider.gameObject) && collider.gameObject.tag == _playerTag) {
                if(this.gameObject.tag == "RopeTop") {
                    _playerRopePointStateChanged.Raise(RopePointEvent.EnterRopeTop);
                }
                if(this.gameObject.tag == "RopeBottom") {
                    _playerRopePointStateChanged.Raise(RopePointEvent.EnterRopeBottom);
                }
            }   
        }

        void OnTriggerExit(Collider collider) {
            if(_playerLayerMask.MMContains(collider.gameObject) && collider.gameObject.tag == _playerTag) {
                if(this.gameObject.tag == "RopeTop") {
                    _playerRopePointStateChanged.Raise(RopePointEvent.ExitRopeTop);
                }

                if(this.gameObject.tag == "RopeBottom") {
                    _playerRopePointStateChanged.Raise(RopePointEvent.ExitRopeBottom);
                }
            }
        }

        public enum RopePointEvent {
            EnterRopeTop,
            EnterRopeBottom,
            ExitRopeTop,
            ExitRopeBottom
        }
    }
}
