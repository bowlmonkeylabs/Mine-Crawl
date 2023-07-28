using BML.Scripts.Player;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("item.")]
    public class ItemSpawnCommands : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private GameObject _experiencePrefab;
        
        [Command("Experience", "Spawns amount of experience at player")]
        private void Experience(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Instantiate(_experiencePrefab, _playerController.transform.position, Quaternion.identity);
            }
            
        }
    }
}