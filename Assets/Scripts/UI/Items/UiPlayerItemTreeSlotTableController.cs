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

        [SerializeField, AssetsOnly] private GameObject _uiPlayerItemCounterPrefab;
        [SerializeField] private RectTransform _uiTableRoot; 

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            // GenerateTreeSlotsTable();
            // TODO do we need to do this on generate? the slots should be dynamically referencing the inventory anyway.

            _playerInventory.PassiveStackableItems.OnItemAdded += OnPassiveStackableItemAdded;
            _playerInventory.PassiveStackableItems.OnItemRemoved += OnPassiveStackabkeItemRemoved;
        }
        
        private void OnDisable()
        {
            _playerInventory.PassiveStackableItems.OnItemAdded -= OnPassiveStackableItemAdded;
            _playerInventory.PassiveStackableItems.OnItemRemoved -= OnPassiveStackabkeItemRemoved;
        }

        #endregion

        private void OnPassiveStackableItemAdded(PlayerItem playerItem)
        {
            // GenerateTreeSlotsTable();
        }

        private void OnPassiveStackabkeItemRemoved(PlayerItem playerItem)
        {
            // GenerateTreeSlotsTable();
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