using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using KinematicCharacterController;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.Player.Items.PlayerItems
{
    [CommandPrefix("cave.")]
    public class CaveCommands : MonoBehaviour
    {
        #region Gizmos

        [SerializeField] private BoolVariable _showCaveGizmos;
        
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

        #endregion
        
        #region Map

        [SerializeField] private GameObjectSceneReference _caveGenerator;

        [Command("map-all")]
        private string RevealMap(bool alsoSetVisited = false)
        {
            var caveGenerator = _caveGenerator.CachedComponent as CaveGenComponentV2;
            caveGenerator.SetAllMapped(alsoSetVisited);
            return "";
        }
        
        #endregion

    }
}