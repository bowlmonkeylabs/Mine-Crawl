using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public interface IHasSlotType<TSlotType> where TSlotType : System.Enum
    {
        TSlotType SlotTypeFilter { get; }
    }
    
    [Serializable]
    public class ItemSlot<TItem, TSlotType> where TItem : IHasSlotType<TSlotType> where TSlotType : System.Enum
    {
        [HideLabel] [HideReferenceObjectPicker]
        [InlineProperty] public TItem Item;

        [TableColumnWidth(30, false)]
        public bool Lock;

        [TableColumnWidth(60, false)]
        public TSlotType Filter;

        public ItemSlot(TItem item)
        {
            Item = item;
            Lock = false;
            Filter = default;
        }

        public bool ResetToDefault(ItemSlot<TItem, TSlotType> defaultValue)
        {
            if (defaultValue == null)
            {
                return false;
            }
            
            Item = defaultValue.Item;
            Lock = defaultValue.Lock;
            Filter = defaultValue.Filter;
            
            return true;
        }
    }
    
    [Serializable]
    public class ItemSlotType<TItem, TSlotType> : IEnumerable<TItem> where TItem : IHasSlotType<TSlotType> where TSlotType : Enum
    {
        #region Inspector
        
        [SerializeField]
        // [OnValueChanged("OnSlotsLimitChangedInInspector")]
        // [OnVariableChanged("OnSlotsLimitChangedInInspector")]
        [InlineButton("OnSlotsLimitChangedInInspector", "Apply")]
        [HorizontalGroup("Group", 0.7f)]
        private SafeIntValueReference _slotsLimit;
            
        private int _slotsActual => _slotsLimit.Value == 0 ? _slots.Count : _slotsLimit.Value;
        
        [SerializeField]
        [Title("Replace cooldown"), HideLabel]
        [HorizontalGroup("Group", 0.25f, marginLeft: 10)]
        private float _itemReplacementCooldown = 2.5f;

        [SerializeField, RequiredListLength("@_slotsActual")]
        [OnValueChanged("OnSlotsChangedInInspector")]
        // [LabelText("")] // TODO better label ?
        [TableList(AlwaysExpanded = true)]
        private List<ItemSlot<TItem, TSlotType>> _slots = new List<ItemSlot<TItem, TSlotType>>();

        [SerializeField] private bool _preserveOrder = true;

        private void OnSlotsLimitChangedInInspector()
        {
            #warning this should also run when the _slotsLimit value changes at runtime... especially when it's referencing a Variable
            if (_slotsActual != _slots.Count)
            {
                _slots.SetLength(_slotsActual);
            }
        }

        [Button("Update"), PropertyOrder(-1)]
        internal void OnSlotsChangedInInspector()
        {
            OnAnyItemChangedInInspector?.Invoke();
        }
        
        #endregion
        
        #region Public interface

        public List<ItemSlot<TItem, TSlotType>> Slots => _slots;
        public List<TItem> Items => _slots.Where(s => s != null && s.Item != null).Select(s => s.Item).ToList();
        public int ItemCount => _slots.Count(s => s != null && s.Item != null);
        public int SlotCount => _slots.Count;

        public TItem this[int key]
        {
            get => _slots[key].Item;
            set => _slots[key].Item = value;
        }

        #region IEnumerable

        public IEnumerator<TItem> GetEnumerator()
        {
            return this.Slots
                .Where(s => s.Item != null)
                .Select(s => s.Item)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private LTDescr _itemReplacementCooldownTween;

        private void RestartItemReplacementCooldown()
        {
            if (_itemReplacementCooldownTween != null)
            {
                LeanTween.cancel(_itemReplacementCooldownTween.uniqueId);
                _itemReplacementCooldownTween = null;
            }
            _itemReplacementCooldownTween = LeanTween.value(0, 1, _itemReplacementCooldown).setOnComplete(() =>
            {
                _itemReplacementCooldownTween = null;
                OnReplacementCooldownTimerStartedOrFinished?.Invoke();
            });
        }

        public (bool canAdd, TItem willReplaceItem) CheckIfCanAddItem(TItem item, bool ignoreReplacementCooldown = false)
        {
            var itemHasFlags = !item?.SlotTypeFilter?.Equals(default(TSlotType)) ?? false;
            Func<ItemSlot<TItem, TSlotType>, bool> FilterPredicate = (itemSlot) => (
                (itemHasFlags && itemSlot.Filter.HasFlag(item.SlotTypeFilter))
                || (!itemHasFlags && itemSlot.Filter.Equals(default(TSlotType)))
            );

            var firstEmptySlot = _slots.FirstOrDefault(s => s.Item == null && FilterPredicate(s));
            if (firstEmptySlot != null)
            {
                return (true, default(TItem));
            }

            if (_slotsLimit.Value == 0)
            {
                return (true, default(TItem));
            }
            
            var firstUnlockedSlot = _slots.FirstOrDefault(s => !s.Lock && FilterPredicate(s));
            if (firstUnlockedSlot != null)
            {
                if (_itemReplacementCooldownTween != null && !ignoreReplacementCooldown)
                {
                    return (false, default(TItem));
                }
                return (true, firstUnlockedSlot.Item);
            }
            return (false, default(TItem));
        }

        public bool TryAddItem(TItem item, bool ignoreReplacementCooldown = false, bool dropOverflow = false)
        {
            var itemHasFlags = !item.SlotTypeFilter.Equals(default(TSlotType));
            Func<ItemSlot<TItem, TSlotType>, bool> FilterPredicate = (itemSlot) => (
                (itemHasFlags && itemSlot.Filter.HasFlag(item.SlotTypeFilter))
                || (!itemHasFlags && itemSlot.Filter.Equals(default(TSlotType)))
            );

            var enumeratedEmptySlots = _slots
                .Select((slot, index) => (slot, index))
                .Where(t => t.slot.Item == null)
                .Select(t => (t.slot, index: (int?)t.index, doesPassFilter: FilterPredicate(t.slot)))
                .ToList();
            var firstEmptySlotThatItemCanGoIn = enumeratedEmptySlots.FirstOrDefault(t => t.doesPassFilter);
            if (firstEmptySlotThatItemCanGoIn.slot != null)
            {
                if (!_preserveOrder)
                {
                    var actualFirstEmptySlot = enumeratedEmptySlots.FirstOrDefault(t => !t.doesPassFilter);
                    if (actualFirstEmptySlot.index.HasValue && actualFirstEmptySlot.index.Value < firstEmptySlotThatItemCanGoIn.index)
                    {
                        _slots[actualFirstEmptySlot.index.Value] = firstEmptySlotThatItemCanGoIn.slot;
                        _slots[firstEmptySlotThatItemCanGoIn.index.Value] = actualFirstEmptySlot.slot;
                    }
                }

                firstEmptySlotThatItemCanGoIn.slot.Item = item;
                if (item != null)
                {
                    RestartItemReplacementCooldown();
                    OnItemAdded?.Invoke(item);
                    OnReplacementCooldownTimerStartedOrFinished?.Invoke();
                }
                return true;
            }

            if (dropOverflow)
            {
                OnItemOverflowed?.Invoke(item);
                return true;
            }

            if (_slotsLimit.Value == 0)
            {
                _slots.Add(new ItemSlot<TItem, TSlotType>(item));
                RestartItemReplacementCooldown();
                OnItemAdded?.Invoke(item);
                OnReplacementCooldownTimerStartedOrFinished?.Invoke();
                return true;
            }
            
            var firstUnlockedSlot = _slots.FirstOrDefault(s => !s.Lock && FilterPredicate(s));
            if (firstUnlockedSlot != null)
            {
                if (_itemReplacementCooldownTween != null && !ignoreReplacementCooldown)
                {
                    return false;
                }

                var prevItem = firstUnlockedSlot.Item;
                // TODO drop the prev item on the ground?
                if (prevItem != null)
                {
                    OnItemRemoved?.Invoke(prevItem);
                    OnItemReplaced?.Invoke(prevItem, item);
                }
                firstUnlockedSlot.Item = item;
                if (item != null)
                {
                    RestartItemReplacementCooldown();
                    OnItemAdded?.Invoke(item);
                    OnReplacementCooldownTimerStartedOrFinished?.Invoke();
                }
                return true;
            }
            return false;
        }

        public bool TryRemoveItem(TItem item)
        {
            // TODO is this right?
            var itemSlot = _slots.FirstOrDefault(s => s.Item.Equals(item)); // Checking only for REFERENCE equality.
            return TryRemoveItemFromSlot(itemSlot);
        }

        public bool TryRemoveItem(int index)
        {
            var itemSlot = _slots[index];
            return TryRemoveItemFromSlot(itemSlot);
        }

        private bool TryRemoveItemFromSlot(ItemSlot<TItem, TSlotType> itemSlot)
        {
            if (itemSlot != null)
            {
                var prevItem = itemSlot.Item;
                itemSlot.Item = default(TItem);
                if (prevItem != null)
                {
                    if (_itemReplacementCooldownTween != null)
                    {
                        LeanTween.cancel(_itemReplacementCooldownTween.uniqueId);
                        _itemReplacementCooldownTween = null;
                        OnReplacementCooldownTimerStartedOrFinished?.Invoke();
                    }
                    OnItemRemoved?.Invoke(prevItem);
                }
                return true;
            }
            return false;
        }

        public bool Clear()
        {
            _slots.SetLength(_slotsLimit.Value);
            foreach (var itemSlot in _slots.Where(s => !s.Lock))
            {
                itemSlot.Item = default(TItem);
            }
            return true;
        }

        public bool ResetToDefault(ItemSlotType<TItem, TSlotType> defaultValues)
        {
            if (defaultValues == null)
            {
                return false;
            }
            
            _itemReplacementCooldown = defaultValues._itemReplacementCooldown;
            _preserveOrder = defaultValues._preserveOrder;
            
            _slotsLimit.Value = defaultValues._slotsLimit.Value;
            _slots.SetLength(_slotsLimit.Value);
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].ResetToDefault(defaultValues._slots[i]);
            }
            
            return true;
        }
        
        #endregion
        
        #region Events

        public delegate void OnSlotItemChanged<T>(T item);
        public delegate void OnSlotItemChanged();
        public delegate void OnSlotItemOverflowed<T>(T item);
        public delegate void OnSlotItemReplaced<T>(T oldItem, T newItem);
        
        public event OnSlotItemChanged<TItem> OnItemAdded;
        public event OnSlotItemChanged<TItem> OnItemRemoved;
        public event OnSlotItemReplaced<TItem> OnItemReplaced;
        public event OnSlotItemOverflowed<TItem> OnItemOverflowed;
        public event OnSlotItemChanged OnAnyItemChangedInInspector;     // When changes happen through the inspector, we don't know which specific item changed
        public event OnSlotItemChanged OnReplacementCooldownTimerStartedOrFinished;

        #endregion
        
    }
}