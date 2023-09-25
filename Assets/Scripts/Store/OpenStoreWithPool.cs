using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.UI;
using BML.Scripts.UI.Items;
using UnityEngine;

namespace BML.Scripts
{
    public class OpenStoreWithPool : MonoBehaviour
    {
        [SerializeField] private StoreItemPoolType _storeItemPoolType;
        [SerializeField] private DynamicGameEvent _onSetStorePool;

        public void OpenStore()
        {
            _onSetStorePool.Raise(_storeItemPoolType);
        }
    }
}