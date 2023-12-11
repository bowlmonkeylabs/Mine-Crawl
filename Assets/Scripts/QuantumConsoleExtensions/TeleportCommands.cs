using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using KinematicCharacterController;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.Player.Items.PlayerItems
{
    [CommandPrefix("tp.")]
    public class TeleportCommands : MonoBehaviour
    {
        [SerializeField] private KinematicCharacterMotor _playerCharacterMotor;

        private const string TELEPORT_TAG = "Dev_Teleport";
        private const string TELEPORT_NAME_PREFIX = "TP_";

        // In order to be found, teleport points should be tagged with 'Dev_Teleport' and should be named with the convention '{TP_<name>}'
        private Transform FindTpPoint(string tpPointName)
        {
            var taggedTeleportPoints = GameObject.FindGameObjectsWithTag(TELEPORT_TAG);
            var requested = taggedTeleportPoints.FirstOrDefault(go =>
            {
                var goTrimmedName = go.name.Trim('{', '}').Remove(0, TELEPORT_NAME_PREFIX.Length);
                return goTrimmedName.Equals(tpPointName, StringComparison.OrdinalIgnoreCase);
            });

            return requested?.transform;
        }

        [Command("waypoint")]
        private string TpToName(string tpPointName)
        {
            var tpPoint = FindTpPoint(tpPointName);
            if (tpPoint == null)
            {
                return "No teleport point with that name.";
            }
            _playerCharacterMotor.SetPositionAndRotation(tpPoint.position, tpPoint.rotation);
            return "";
        }

        [Command("start")]
        private string TpToStart()
        {
            return TpToName("start");
        }
        
        [Command("end")]
        private string TpToEnd()
        {
            return TpToName("end");
        }
        
        [Command("merchant")]
        private string TpToMerchant()
        {
            return TpToName("merchant");
        }
        
    }
}