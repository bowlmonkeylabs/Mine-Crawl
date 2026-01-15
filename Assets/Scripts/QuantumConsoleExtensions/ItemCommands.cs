using BML.Scripts.Player;
using BML.Scripts.Player.Items;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using QFSW.QC;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("item.")]
    public class ItemCommands : SerializedMonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField, Tooltip("Root path to scan for items, relative to Assets folder")] 
        private string _itemsRootPath = "/Entities/Items/";
        [SerializeField] private Dictionary<string, PlayerItem> _knownItems;
        [SerializeField, ReadOnly] private string _listKnownItemsFormattedString;

        [Button("Catalog Item Assets")]
        private void CatalogKnownItems()
        {
            var items = UnityEditor.AssetDatabase.FindAssets("t:PlayerItem", new[] { "Assets" + _itemsRootPath });
            _knownItems = new Dictionary<string, PlayerItem>();
            var formattedNames = new List<string>();
            foreach (var itemGUID in items)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(itemGUID);
                var item = UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerItem>(path);

                // Generate easily typed name from file name
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var itemName = fileName;
                if (fileName.StartsWith("PlayerItem_", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = fileName.Substring("PlayerItem_".Length);
                }
                itemName = itemName.Replace(' ', '-').Replace('_', '-');
                var itemNameLower = itemName.ToLowerInvariant();

                if (item != null && !_knownItems.ContainsKey(itemNameLower))
                {
                    _knownItems.Add(itemNameLower, item);
                    formattedNames.Add(itemName);
                }
            }

            var itemNames = formattedNames.OrderBy(name => name);
            _listKnownItemsFormattedString = String.Join(", ", itemNames);
        }

        [Command("list", "Lists all known items.")]
        private void ListItems()
        {
            if (_knownItems == null || _listKnownItemsFormattedString == null)
            {
                Debug.LogError("Known items not cataloged yet. Run CatalogKnownItems first.");
                return;
            }

            Debug.Log("Known items: " + _listKnownItemsFormattedString);
        }

        [Command("give", "Gives the player an item by name.")]
        private void GiveItem(string itemName)
        {
            if (_knownItems == null)
            {
                Debug.LogError("Known items not cataloged yet. Run CatalogKnownItems first.");
                return;
            }

            if (!_knownItems.TryGetValue(itemName.ToLowerInvariant(), out PlayerItem item))
            {
                Debug.LogError($"Item with name '{itemName}' not found.");
                return;
            }

            _playerInventory.TryAddItem(item);
            Debug.Log($"Gave item '{itemName}' to player.");
        }        
    }
}