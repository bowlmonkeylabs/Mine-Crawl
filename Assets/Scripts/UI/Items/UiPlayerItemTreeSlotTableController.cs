using System;
using System.Linq;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.UI.Items
{
    public class UiPlayerItemTreeSlotTableController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private ItemType _itemType = ItemType.PassiveStackable;

        [SerializeField, AssetsOnly] private GameObject _uiPlayerItemCounterPrefab;
        [SerializeField] private RectTransform _uiTableRoot;
        [SerializeField] private bool _generateAtRuntime = false;

        private bool GenerateAtRuntime => _generateAtRuntime
            && _itemType != ItemType.PassiveStackable;
        // Disabled for PassiveStackable now, since we decided to have a fixed number of slots in the UI.
        // Generate in the editor via the button below if needed.;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            if (GenerateAtRuntime)
            {
                GenerateTreeSlotsTable();
                _playerInventory.PassiveStackableItems.OnItemAdded += OnPassiveStackableItemAdded;
                _playerInventory.PassiveStackableItems.OnItemRemoved += OnPassiveStackableItemRemoved;
            }
        }
        
        private void OnDisable()
        {
            if (GenerateAtRuntime)
            {
                _playerInventory.PassiveStackableItems.OnItemAdded -= OnPassiveStackableItemAdded;
                _playerInventory.PassiveStackableItems.OnItemRemoved -= OnPassiveStackableItemRemoved;
            }
        }

        #endregion

        private void OnPassiveStackableItemAdded(PlayerItem playerItem)
        {
            GenerateTreeSlotsTable();
        }

        private void OnPassiveStackableItemRemoved(PlayerItem playerItem)
        {
            GenerateTreeSlotsTable();
        }

        [Button]
        private void GenerateTreeSlotsTable()
        {
            // if (_playerInventory.PassiveStackableItemTrees.Count > _uiTableRoot.childCount)
            // {
            //     Debug.LogError("Not enough slots to display all slotted item trees.");
            // }
            
            for (int i = 0; i < _uiTableRoot.childCount; i++)
            {
                var childTransform = _uiTableRoot.GetChild(i);
                var uiPlayerItemCounterController = childTransform.GetComponentInChildren<UiPlayerItemCounterController>();
                if (uiPlayerItemCounterController != null)
                {
                    uiPlayerItemCounterController.SetDisplayPassiveStackableTreeSlotFromInventory(i);
                }
            }
        }
    }
}