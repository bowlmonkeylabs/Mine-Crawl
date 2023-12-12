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
    [Serializable]
    public class ItemSlotType<T> : IEnumerable<T>
    {
        [Serializable]
        public class ItemSlot<TItem>
        {
            [HideLabel] [HideReferenceObjectPicker]
            [InlineProperty] public TItem Item;

            [TableColumnWidth(30, false)]
            public bool Lock;

            public ItemSlot(TItem item)
            {
                Item = item;
                Lock = false;
            }
        }
        
        #region Inspector
        
        [SerializeField]
        // [OnValueChanged("OnSlotsLimitChangedInInspector")]
        // [OnVariableChanged("OnSlotsLimitChangedInInspector")]
        [InlineButton("OnSlotsLimitChangedInInspector", "Set")]
        private SafeIntValueReference _slotsLimit;
            
        private int _slotsActual => _slotsLimit.Value == 0 ? _slots.Count : _slotsLimit.Value;

        [SerializeField, RequiredListLength("@_slotsActual")]
        [OnValueChanged("OnSlotsChangedInInspector")]
        // [LabelText("")] // TODO better label ?
        [TableList(AlwaysExpanded = true)]
        private List<ItemSlot<T>> _slots = new List<ItemSlot<T>>();

        private void OnSlotsLimitChangedInInspector()
        {
            #warning this should also run when the _slotsLimit value changes at runtime... especially when it's referencing a Variable
            if (_slotsActual != _slots.Count)
            {
                _slots.SetLength(_slotsActual);
            }
        }

        [Button("Update")]
        private void OnSlotsChangedInInspector()
        {
            OnAnyItemChangedInInspector?.Invoke();
        }
        
        #endregion
        
        #region Public interface

        public List<ItemSlot<T>> Slots => _slots;
        public List<T> Items => _slots.Where(s => s.Item != null).Select(s => s.Item).ToList();
        public int ItemCount => _slots.Count(s => s.Item != null);
        public int SlotCount => _slots.Count;

        public T this[int key]
        {
            get => _slots[key].Item;
            set => _slots[key].Item = value;
        }

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
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

        public bool TryAddItem(T item)
        {
            var firstEmptySlot = _slots.FirstOrDefault(s => s.Item == null);
            if (firstEmptySlot != null)
            {
                firstEmptySlot.Item = item;
                if (item != null)
                {
                    OnItemAdded?.Invoke(item);
                }
                return true;
            }

            if (_slotsLimit.Value == 0)
            {
                _slots.Add(new ItemSlot<T>(item));
                return true;
            }
            
            var firstUnlockedSlot = _slots.FirstOrDefault(s => !s.Lock);
            if (firstUnlockedSlot != null)
            {
                var prevItem = firstUnlockedSlot.Item;
                // TODO drop the prev item on the ground?
                if (prevItem != null)
                {
                    OnItemRemoved?.Invoke(prevItem);
                }
                firstUnlockedSlot.Item = item;
                if (item != null)
                {
                    OnItemAdded?.Invoke(item);
                }
                return true;
            }
            return false;
        }

        public bool TryRemoveItem(T item)
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

        private bool TryRemoveItemFromSlot(ItemSlot<T> itemSlot)
        {
            if (itemSlot != null)
            {
                var prevItem = itemSlot.Item;
                itemSlot.Item = default(T);
                if (prevItem != null)
                {
                    OnItemRemoved?.Invoke(prevItem);
                }
                return true;
            }
            return false;
        }

        public bool Clear()
        {
            foreach (var itemSlot in _slots.Where(s => !s.Lock))
            {
                itemSlot.Item = default(T);
            }
            return true;
        }

        public delegate void OnSlotItemChanged<T>(T item);
        public delegate void OnSlotItemChanged();
        
        public event OnSlotItemChanged<T> OnItemAdded;                  // T is the item that was ADDED
        public event OnSlotItemChanged<T> OnItemRemoved;                // T is the item that was REMOVED
        public event OnSlotItemChanged OnAnyItemChangedInInspector;     // When changes happen through the inspector, we don't know which specific item changed

        #endregion
    }
}