using BML.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using Random = UnityEngine.Random;

namespace BML.Scripts.UI
{
    public class UiStoreCanvasController : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("CaveGen")] private GameObjectSceneReference _caveGeneratorGameObjectSceneReference;
        private CaveGenComponentV2 _caveGenerator => _caveGeneratorGameObjectSceneReference?.CachedComponent as CaveGenComponentV2;
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private Transform _listContainerStoreButtons;
        [SerializeField] private int _maxItemsShown = 0;
        [SerializeField] private bool _randomizeStoreOnBuy;
        [SerializeField] private bool _filterOutMaxedItems;
        [SerializeField] private Button _buttonNavLeft;
        [SerializeField] private Button _buttonNavRight;
        [SerializeField] private StoreInventory _storeInventory;

        private List<Button> buttonList = new List<Button>();

        private void Awake()
        {
            #warning Remove this once we're done working on the stores/inventory
            GenerateStoreItems();
        }

        void OnEnable() {
            _onPurchaseEvent.Subscribe(OnBuy);
        }

        void OnDisable() {
            _onPurchaseEvent.Unsubscribe(OnBuy);
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyShopItems();

            List<StoreItem> shownStoreItems = _storeInventory.StoreItems;

            if(_filterOutMaxedItems) {
                shownStoreItems = shownStoreItems.Where(si => !si._hasMaxAmount || (si._playerInventoryAmount.Value < si._maxAmount.Value)).ToList();
            }

            if(_randomizeStoreOnBuy) {
                Random.InitState(SeedManager.Instance.GetSteppedSeed("UpgradeStore"));
                shownStoreItems = shownStoreItems.OrderBy(c => Random.value).ToList();
            }

            if(_maxItemsShown > 0) {
                shownStoreItems = shownStoreItems.Take(_maxItemsShown).ToList();
            }

            if(shownStoreItems.Count > _listContainerStoreButtons.childCount - 1) {
                Debug.LogError("Store does not have enough buttons to display options");
                return;
            }

            for(int i = 0; i < shownStoreItems.Count; i++) {
                GameObject buttonGameObject = _listContainerStoreButtons.GetChild(i).gameObject;
                buttonGameObject.GetComponent<UiStoreButtonController>().Init(shownStoreItems[i]);
                buttonGameObject.SetActive(true);
                buttonList.Add(buttonGameObject.GetComponent<Button>());
            }

            //Resume button will always be last in list
            buttonList.Add(_listContainerStoreButtons.GetChild(_listContainerStoreButtons.childCount - 1).GetComponent<Button>());

            SetNavigationOrder();
        }

        [Button("Destroy Store Items")]
        public void DestroyShopItems()
        {
            buttonList.Clear();
            
            for(int i = 0; i < _listContainerStoreButtons.childCount - 1; i++) {
                _listContainerStoreButtons.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void SetNavigationOrder()
        {
            for(int i = 0; i < buttonList.Count; i++) {
                Button prevButton = i > 0 ? buttonList[i - 1] : buttonList[buttonList.Count - 1];
                Button nextButton = i < buttonList.Count - 1 ? buttonList[i + 1] : buttonList[0];
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = prevButton;
                nav.selectOnDown = nextButton;
                if(_buttonNavLeft != null) nav.selectOnLeft = _buttonNavLeft;
                if(_buttonNavRight != null) nav.selectOnRight = _buttonNavRight;
                buttonList[i].navigation = nav;
            }
        }

        protected void OnBuy(object prevStoreItem, object storeItem) {
            SeedManager.Instance.UpdateSteppedSeed("UpgradeStore");
            if(_randomizeStoreOnBuy) {
                GenerateStoreItems();
            }
        }
    }
}