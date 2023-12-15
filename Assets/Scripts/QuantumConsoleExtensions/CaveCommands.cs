using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using KinematicCharacterController;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.Player.Items.PlayerItems
{
    [CommandPrefix("cave.")]
    public class CaveCommands : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private BoolVariable _showCaveGizmos;

        #endregion
        
        [Command("gizmos")]
        private string ToggleShowGizmos()
        {
            return SetShowGizmos(!_showCaveGizmos.Value);
        }
        
        
        [Command("gizmos")]
        private string SetShowGizmos(bool showGizmos)
        {
            _showCaveGizmos.Value = showGizmos;
            return $"Cave gizmos {(showGizmos ? "enabled" : "disabled")}.";
        }

    }
}