using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using QFSW.QC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.PlayerLoop;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("graphics.")]
    public static class GraphicsCommands
    {   
        [Command("targetFrameRate", "Sets the Unity Application.targetFrameRate")]
        private static void SetTargetFrameRate(int target)
        {
            Application.targetFrameRate = target;
        }
    }
}