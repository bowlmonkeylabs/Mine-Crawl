using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Utils;
using BML.Scripts.ScriptableObjectVariables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "VariableContainer", menuName = "BML/Variables/VariableContainer MINE CRAWL", order = 0)]
    public class MineCrawlVariableContainer : VariableContainer
    {
        [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
        [SerializeField] private List<ItemLootTableVariable> ItemLootTableVariables = new List<ItemLootTableVariable>();
        
        public List<ItemLootTableVariable> GetItemLootTableVariables() => ItemLootTableVariables;

    #if UNITY_EDITOR

        [GUIColor(0, 1, 0)]
        [TitleGroup("Populate Container"), PropertyOrder(0), ShowIf("@_populateMode != ContainerPopulateMode.Manual")]
        [Button(ButtonSizes.Large), DisableIf("@(_populateMode == ContainerPopulateMode.Folder && string.IsNullOrEmpty(FolderPath))")]
        public override void PopulateContainer()
        {
            base.PopulateContainer();

            ItemLootTableVariables.Clear();

            if (_populateMode == ContainerPopulateMode.Folder)
            {
                ItemLootTableVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<ItemLootTableVariable>(FolderPath, IncludeSubdirectories).ToList();
            }
            else if (_populateMode == ContainerPopulateMode.ContainerKey)
            {
                // Only populate lists of resettable variable types
                ItemLootTableVariables = AssetDatabaseUtils.FindAndLoadAssetsOfType<ItemLootTableVariable>().Where(variable => (variable.IncludeInContainers & ContainerKey) == ContainerKey).ToList();
            }

            Debug.Log($"PopulateContainer {this.name}: " +
                      $" | {ItemLootTableVariables.Count} Item Loot tables");
        }

    #endif
        
    }


}

