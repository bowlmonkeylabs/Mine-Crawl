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
        [SerializeField] private PlayerController _playerController;

        [Command("waypoint")]
        private string TpToWaypoint(string tpPointName)
        {
            try
            {
                _playerController.TpToWaypoint(tpPointName);
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [Command("start")]
        private string TpToStart()
        {
            return TpToWaypoint("start");
        }
        
        [Command("end")]
        private string TpToEnd()
        {
            return TpToWaypoint("end");
        }
        
        [Command("merchant")]
        private string TpToMerchant()
        {
            return TpToWaypoint("merchant");
        }
        
    }
}