using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.Utilities;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items
{
    public enum ItemType {
        PassiveStackable,
        Active,
        Passive
    }

    public enum ItemEffectTrigger {
        WhenAcquiredOrActivated,
        RecurringTimer,
        OnDash,
        OnPickaxeSwing,
        OnPickaxeSwingHit,
        OnPickaxeSweep,
        OnPickaxeSweepHit,
        OnPickaxeSwingCrit,
    }

    public enum ItemEffectType {
        StatIncrease,
        FireProjectile,
        ChangeLootTable,
        ToggleBoolVariable,
        InstantiatePrefab
    }

    [Serializable, InlineEditor]
    public class ItemEffect {
        public ItemEffectTrigger Trigger = ItemEffectTrigger.WhenAcquiredOrActivated;

        public bool UseActivationLimit = false;
        [ShowIf("UseActivationLimit")] public IntVariable RemainingActivations;
        
        public bool UseActivationCooldownTimer = false;
        [ShowIf("UseActivationCooldownTimer")] public TimerVariable ActivationCooldownTimer;

        [ShowIfGroup("RecurringTimer", Condition = "Trigger", Value = ItemEffectTrigger.RecurringTimer)] public float Time;
        [HideInInspector] public float LastTimeCheck;

        public ItemEffectType Type = ItemEffectType.StatIncrease;

        [ShowIfGroup("StatIncrease", Condition = "Type", Value = ItemEffectType.StatIncrease)]
        [ShowIfGroup("StatIncrease")] public bool UsePercentageIncrease = false;
        [ShowIfGroup("StatIncrease")] public bool IsTemporaryStatIncrease = false;
        [ShowIfGroup("StatIncrease"), ShowIf("IsTemporaryStatIncrease")] public float TemporaryStatIncreaseTime;
        [ShowIfGroup("StatIncrease"), ShowIf("UsePercentageIncrease")] public FloatVariable FloatStat;
        [ShowIfGroup("StatIncrease"), HideIf("UsePercentageIncrease")] public IntVariable IntStat;
        [ShowIfGroup("StatIncrease"), HideIf("UsePercentageIncrease")] public int StatIncreaseAmount;
        [ShowIfGroup("StatIncrease"), ShowIf("UsePercentageIncrease")] public float StatIncreasePercent;

        [ShowIfGroup("FireProjectile", Condition = "Type", Value = ItemEffectType.FireProjectile)]
        [ShowIfGroup("FireProjectile")] public GameObject ProjectilePrefab;

        [FormerlySerializedAs("LootTableToOverride")]
        [ShowIfGroup("ChangeLootTable", Condition = "Type", Value = ItemEffectType.ChangeLootTable)]
        
        [ShowIfGroup("ChangeLootTable")] public LootTableVariable LootTableVariable;
        private bool _lootTableKeyDoesNotExist => LootTableVariable != null && LootTableVariable.Value != null && !LootTableVariable.Value.HasKey(LootTableKey);
        [ShowIfGroup("ChangeLootTable")]
        [InfoBox("Key does not exist in selected loot table.", InfoMessageType.Error, visibleIfMemberName:"_lootTableKeyDoesNotExist")]
        public LootTableKey LootTableKey;
        [ShowIfGroup("ChangeLootTable")] public float LootTableAddAmount;

        [ShowIfGroup("ToggleBoolVariable", Condition = "Type", Value = ItemEffectType.ToggleBoolVariable)]
        [ShowIfGroup("ToggleBoolVariable")] public BoolVariable BoolVariableToToggle;

        [ShowIfGroup("InstantiatePrefab", Condition = "Type", Value = ItemEffectType.InstantiatePrefab)]
        [ShowIfGroup("InstantiatePrefab")] public GameObject PrefabToInstantiate;
        [ShowIfGroup("InstantiatePrefab")] public TransformSceneReference InstantiatePrefabPositionTransform;
        [ShowIfGroup("InstantiatePrefab")] public TransformSceneReference InstantiatePrefabContainer;

        public void Reset()
        {
            LastTimeCheck = Mathf.NegativeInfinity;
            RemainingActivations?.Reset();
            ActivationCooldownTimer?.ResetTimer();
        }
    }

    public delegate void OnAffordabilityChanged();

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerItem", menuName = "BML/Player/PlayerItem", order = 0)]
    public class PlayerItem : SerializedScriptableObject, IResettableScriptableObject
    {
        [SerializeField, FoldoutGroup("Descriptors")] private string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _effectDescription;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _storeDescription;
        [SerializeField, FoldoutGroup("Descriptors")] private bool _useIconColor;
        [SerializeField, FoldoutGroup("Descriptors"), ShowIf("_useIconColor")] private Color _iconColor;
        [SerializeField, FoldoutGroup("Descriptors"), PreviewField(100, ObjectFieldAlignment.Left)] private Sprite _icon;

        [
            DictionaryDrawerSettings(KeyLabel = "Resource", ValueLabel = "Amount",
            DisplayMode = DictionaryDisplayOptions.ExpandedFoldout),
            SerializeField
        ] private Dictionary<PlayerResource, int> _itemCost = new Dictionary<PlayerResource, int>();

        [SerializeField, FoldoutGroup("Effect")] private ItemType _itemType = ItemType.PassiveStackable;
        [SerializeField, FoldoutGroup("Effect")] private List<ItemEffect> _itemEffects;

        private void OnEnable() {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged += InvokeOnAffordabilityChanged;
            });
        }

        private void OnDisable() {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged -= InvokeOnAffordabilityChanged;
            });
        }

        private void InvokeOnAffordabilityChanged() {
            OnAffordabilityChanged?.Invoke();
        }

        public void ResetScriptableObject()
        {
            _itemEffects.ForEach(e => e.Reset());
        }

        public Sprite Icon { get => _icon; }
        public bool UseIconColor { get => _useIconColor; }
        public Color IconColor { get => _iconColor; }
        public string Name { get => _name; }
        public string EffectDescription { get => _effectDescription; }
        public string StoreDescription { get => _storeDescription; }
        public List<ItemEffect> ItemEffects { get => _itemEffects; }
        public ItemType Type { get => _itemType; }

        public event OnAffordabilityChanged OnAffordabilityChanged;

        //TODO: return what resources youre short on
        public bool CheckIfCanAfford() {
            return _itemCost.Any((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerCount >= entry.Value);
        }

        public void DeductCosts() {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerCount -= entry.Value);
        }

        public string FormatCostsAsText() {
            return String.Join(" + ", _itemCost.Select((KeyValuePair<PlayerResource, int> entry) => $"{entry.Value}{entry.Key.IconText}"));
        }
    }
}
