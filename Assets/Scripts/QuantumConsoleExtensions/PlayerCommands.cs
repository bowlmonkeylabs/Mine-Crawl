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
        
        [Command("health-temporary", "Sets player temporary health.")]
        private void SetHealthTemporary(int health)
        {
            _playerController.SetHealthTemporary(health);
        }
        
        [Command("revive", "Sets player health back to the starting value.")]
        private void Revive()
        {
            _playerController.Revive();
        }

        [Command("full-heal", "Heals the player's health and temporary health to max.")]
        private void FullHeal()
        {
            _playerController.Heal(999);
            _playerController.HealTemporary(999);
        }
        
        [Command("pickaxe_distance", "Sets player pickaxe interact range.")]
        private void SetPickaxeDistance(float dist)
        {
            _playerController.SetPickaxeDistance(dist);
        }
        
        #endregion
        
    }
}