using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("graphics.")]
    public class GraphicsCommands : MonoBehaviour
    {   
        [Command("targetFrameRate", "Sets the Unity Application.targetFrameRate")]
        private void SetTargetFrameRate(int target)
        {
            Application.targetFrameRate = target;
        }
    }
}