using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Player.Items.Store
{
    public enum StoreId    // Each store ID should be assigned to only ONE store in the scene; it should be treated as a unique ID.
    {
        Upgrade = 1,
        Merchant = 2,
        MerchantEnd = 3,
    }
        
    internal enum ItemPoolBehavior
    {
        KeepItem,
        RemoveItem,
        ReplaceItem,
        ReplaceAll,
    }

    public abstract class AbstractItemStoreInventory : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private StoreId _storeId;

        [SerializeField] private int _numAvailableItems = 5;
        [NonSerialized, ShowInInspector, ReadOnly] private List<PlayerItem> _availableItems; // TODO enforce length11

        [SerializeField] private ItemPoolBehavior _buyBehavior;

        [ShowInInspector, ReadOnly]
        private string SteppedSeedKey => $"ItemStoreController_{_storeId.ToString()}"; // this should be unique to this instance of store, but also deterministic, meaning it should work out the same for the same seed.

        #endregion

        #region Unity lifecycle

        private void Start()
        {
            SeedManager.Instance.InitSteppedSeed(SteppedSeedKey);
            
            ReplaceAllItems();
            OnAvailableItemsChanged?.Invoke();
        }

        #endregion

        #region Public interface

        public StoreId StoreId => _storeId;
        public List<PlayerItem> AvailableItems => _availableItems;

        public void BuyItem(PlayerItem item)
        {
            var itemIndex = _availableItems.IndexOf(item);
            BuyItem(itemIndex);
        }

        public void BuyItem(int index)
        {
            switch (_buyBehavior)
            {
                default:
                case ItemPoolBehavior.KeepItem:
                    // do nothing
                    break;
                case ItemPoolBehavior.RemoveItem:
                    RemoveItem(index);
                    OnAvailableItemsChanged?.Invoke();
                    break;
                case ItemPoolBehavior.ReplaceItem:
                    ReplaceItem(index);
                    OnAvailableItemsChanged?.Invoke();
                    break;
                case ItemPoolBehavior.ReplaceAll:
                    ReplaceAllItems();
                    OnAvailableItemsChanged?.Invoke();
                    break;
            }
            
        }

        #endregion
        
        #region Events
        
        public delegate void _OnAvailableItemsChanged();

        public event _OnAvailableItemsChanged OnAvailableItemsChanged;
        
        #endregion
        
        #region Manage Available Items

        private void RemoveItem(int index)
        {
            _availableItems[index] = null;
        }

        private void ReplaceItem(int index)
        {
            var itemPool = GetItemPoolRandomized(false);
            var replacementItem = itemPool.Except(_availableItems).FirstOrDefault();
            _availableItems[index] = replacementItem;
        }

        private void ReplaceAllItems()
        {
            _availableItems = GetItemPoolRandomized().Take(_numAvailableItems).ToList();
            // TODO what to do if _availableItems.Length < _numAvailableItems ?? not enough items left in the pool
        }

        protected void OnItemPoolUpdated()
        {
            // TODO call this
            
            switch (_buyBehavior)
            {
                default:
                case ItemPoolBehavior.KeepItem:
                    // do nothing
                    break;
                case ItemPoolBehavior.RemoveItem:
                case ItemPoolBehavior.ReplaceItem:
                    var itemPool = GetItemPoolRandomized(false);
                    var itemsInStoreThatAreNoLongerInPool = _availableItems
                        .Select((item, index) => (item, index))
                        .Where(i => i.item != null && !itemPool.Contains(i.item)) 
                        .Select(i => i.index)
                        .ToList();
                    if (itemsInStoreThatAreNoLongerInPool.Any())
                    {
                        List<PlayerItem> replacementItems = null;
                        if (_buyBehavior == ItemPoolBehavior.RemoveItem)
                        {
                            replacementItems = itemPool
                                .Except(_availableItems)
                                .Take(itemsInStoreThatAreNoLongerInPool.Count)
                                .ToList();
                        }
                        int replacementIndex = 0;
                        foreach (var indexToReplace in itemsInStoreThatAreNoLongerInPool)
                        {
                            if (replacementIndex < (replacementItems?.Count ?? 0))
                            {
                                _availableItems[indexToReplace] = replacementItems[replacementIndex++];
                            }
                            else
                            {
                                _availableItems[indexToReplace] = null;
                            }
                        }
                    }
                    OnAvailableItemsChanged?.Invoke();
                    break;
                case ItemPoolBehavior.ReplaceAll:
                    ReplaceAllItems();
                    OnAvailableItemsChanged?.Invoke();
                    break;
            }
            
            // TODO update ui
        }
        
        #endregion
        
        #region Get Item Pool

        protected abstract IEnumerable<PlayerItem> GetItemPool();

        protected List<PlayerItem> GetItemPoolRandomized(bool updateSeed = false)
        {
            var itemPool = GetItemPool();
            
            if (updateSeed)
            {
                SeedManager.Instance.UpdateSteppedSeed(SteppedSeedKey);
            }
            var steppedSeed = SeedManager.Instance.GetSteppedSeed(SteppedSeedKey);
            Random.InitState(steppedSeed);

            var randomizedItemPool = itemPool.OrderBy(i => Random.value).ToList(); // .ToList() is important here, in order to enforce Random.value is actually evaluated before this function returns.
            
            return randomizedItemPool;
        }
        
        #endregion
        
    }
}