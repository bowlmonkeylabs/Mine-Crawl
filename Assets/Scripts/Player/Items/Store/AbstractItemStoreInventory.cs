using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2;
using BML.Scripts.Player.Items;
using BML.Scripts.UI.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
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
        [ShowInInspector, ReadOnly]
        private string SteppedSeedKey => $"ItemStoreController_{_storeId.ToString()}"; // this should be unique to this instance of store, but also deterministic, meaning it should work out the same for the same seed.

        [SerializeField] private int _numAvailableItems = 5;
        [NonSerialized, ShowInInspector, ReadOnly] private List<PlayerItem> _availableItems; // TODO enforce length11

        [SerializeField] private ItemPoolBehavior _buyBehavior;

        [Tooltip("When using ReplaceItem or ReplaceAll, this will ensure that the replacement item is different from the item being replaced (when possible). This may not be possible if they are not enough unique items left in the pool.")] 
        [SerializeField, ShowIf("@_buyBehavior == ItemPoolBehavior.ReplaceItem || _buyBehavior == ItemPoolBehavior.ReplaceAll")]
        private bool _guaranteeReplacementIsDifferent = true;

        [SerializeField, Tooltip("This event is needed in order to update the stepped seed at the start of each level the player visits.")] private GameEvent _onAfterGenerateLevelObjects;

        #endregion

        private void RefreshStoreForLevel()
        {
            ReplaceAllItems();
            OnAvailableItemsChanged?.Invoke();
        }

        #region Unity lifecycle

        private void Awake()
        {
            _onAfterGenerateLevelObjects.Subscribe(RefreshStoreForLevel);
        }

        private void OnDestroy()
        {
            _onAfterGenerateLevelObjects.Unsubscribe(RefreshStoreForLevel);
        }

        #endregion

        #region Public interface

        public StoreId StoreId => _storeId;
        public List<PlayerItem> AvailableItems => _availableItems;

        public PlayerItem BuyItem(PlayerItem item)
        {
            var itemIndex = _availableItems.IndexOf(item);
            return BuyItem(itemIndex);
        }

        public PlayerItem BuyItem(int index)
        {
            PlayerItem boughtItem = _availableItems[index];

            bool didAvailableItemsChange = false;
            switch (_buyBehavior)
            {
                default:
                case ItemPoolBehavior.KeepItem:
                    // do nothing
                    break;
                case ItemPoolBehavior.RemoveItem:
                    RemoveItem(index);
                    didAvailableItemsChange = true;
                    break;
                case ItemPoolBehavior.ReplaceItem:
                    ReplaceItem(index);
                    didAvailableItemsChange = true;
                    break;
                case ItemPoolBehavior.ReplaceAll:
                    ReplaceAllItems();
                    didAvailableItemsChange = true;
                    break;
            }

            if (didAvailableItemsChange)
            {
                OnAvailableItemsChanged?.Invoke();
            }

            return boughtItem;
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
            if (!_guaranteeReplacementIsDifferent)
            {
                _availableItems.RemoveAt(index);
            }
            var replacementItem = itemPool.Except(_availableItems).FirstOrDefault();
            _availableItems[index] = replacementItem;
        }

        private void ReplaceAllItems()
        {
            List<PlayerItem> itemPool = GetItemPoolRandomized(true);
            if (_guaranteeReplacementIsDifferent && _availableItems != null)
            {
                var reducedItemPool = itemPool.Except(_availableItems).ToList();
                if (reducedItemPool.Count >= _numAvailableItems)
                {
                    itemPool = reducedItemPool;
                }
            }
            
            _availableItems = itemPool.Take(_numAvailableItems).ToList();
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
                        if (_buyBehavior == ItemPoolBehavior.ReplaceItem)
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
        }
        
        #endregion
        
        #region Get Item Pool

        protected abstract IEnumerable<PlayerItem> GetItemPool();
        
        protected virtual bool SkipRandomizeItemPool => false;

        protected List<PlayerItem> GetItemPoolRandomized(bool updateSeed = false)
        {
            var itemPool = GetItemPool();
            if (SkipRandomizeItemPool)
            {
                return itemPool.ToList();
            }

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