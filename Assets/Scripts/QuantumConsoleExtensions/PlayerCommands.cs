using BML.Scripts.Player;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("player.")]
    public class PlayerCommands : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;

        #region Commands

        [Command("health", "Sets player health.")]
        private void SetHealth(int health)
        {
            _playerController.SetHealth(health);
        }
        
        [Command("revive", "Sets player health back to the starting value.")]
        private void Revive()
        {
            _playerController.Revive();
        }
        
        [Command("pickaxe_distance", "Sets player pickaxe interact range.")]
        private void SetPickaxeDistance(float dist)
        {
            _playerController.SetPickaxeDistance(dist);
        }
        
        #endregion
        
    }
}