using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("xp.")]
    public class ExperienceCommands : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private GameObject _experiencePrefab;

        [Command("spawn", "Spawns amount of experience at player")]
        private void Spawn(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Instantiate(_experiencePrefab, _playerController.transform.position, Quaternion.identity);
            }
        }
        
        [Command("level", "Levels the player by amount")]
        private void Level(int amount)
        {
            _playerController.AddLevel(amount, true);
        }
    }
}