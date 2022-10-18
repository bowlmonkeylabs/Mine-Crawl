using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player;
using QFSW.QC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.PlayerLoop;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("player.")]
    public static class PlayerCommands
    {
        private static string _playerControllerTransformSceneReferenceAddress = "Assets/Entities/Player/PlayerTransformSceneReference.asset";

        [Command("health", "Sets player health.")]
        private static async void DespawnAll(int health)
        {
            var playerController = await GetActivePlayerController();

            playerController.SetHealth(health);
        }
        
        [Command("revive", "Sets player health back to the starting value.")]
        private static async void Revive()
        {
            var playerController = await GetActivePlayerController();

            playerController.Revive();
        }
        
        [Command("pickaxe_distance", "Sets player pickaxe interact range.")]
        private static async void SetPickaxeDistance(float dist)
        {
            var playerController = await GetActivePlayerController();

            playerController.SetPickaxeDistance(dist);
        }

        private static async Task<PlayerController> GetActivePlayerController()
        {
            var asyncHandle =
                Addressables.LoadAssetAsync<TransformSceneReference>(_playerControllerTransformSceneReferenceAddress);
            
            await asyncHandle.Task;
            if (asyncHandle.Result.Value == null)
            {
                throw new Exception($"Player controller scene instance is not assigned.");
            }

            var playerController = asyncHandle.Result.Value.GetComponent<PlayerController>();
            if (playerController == null)
            {
                throw new Exception($"Player controller component not found on referenced scene object.");
            }

            return playerController;
        }
    }
}