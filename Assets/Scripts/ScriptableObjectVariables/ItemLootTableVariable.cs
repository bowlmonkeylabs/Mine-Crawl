using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.ReferenceTypeVariables;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.ScriptableObjectVariables
{
    [Required]
    [CreateAssetMenu(fileName = "ItemLootTableVariable", menuName = "BML/Variables/ItemLootTableVariable", order = 0)]
    public class ItemLootTableVariable : ReferenceTypeVariable<LootTable<LootTableKey, PlayerItem>>
    {
        public string GetName() => name;
        public string GetDescription() => Description;
    }
    
    [Serializable]
    [InlineProperty]
    public class ItemLootTableReference : Reference<LootTable<LootTableKey, PlayerItem>, ItemLootTableVariable> { }
}