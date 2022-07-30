using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;

namespace BML.Scripts
{
    public class DebugUiManager : MonoBehaviour
    {
        [SerializeField] private GameEvent _onOpenDebugUi;
        [SerializeField] private GameObject _debugUi;

        private void OnEnable()
        {
            _onOpenDebugUi.Subscribe(OpenDebugUi);
        }

        private void OnDisable()
        {
            _onOpenDebugUi.Unsubscribe(OpenDebugUi);
        }

        private void OpenDebugUi()
        {
            _debugUi.SetActive(!_debugUi.activeSelf);
        }
    }
}