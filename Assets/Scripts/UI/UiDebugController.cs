using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiDebugController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TransformSceneReference _playerTransformSceneReference;
        [SerializeField] private FloatVariable _currentSpawnDelay;
        [SerializeField] private FloatVariable _currentSpawnCap;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private IntReference _seedDebugReference;

        private void Awake()
        {
            UpdateText();
        }

        private void Update() {
            UpdateText();
        }

        protected void UpdateText()
        {
            _text.text = $@"Player Coordinates: {this.formatVector3(_playerTransformSceneReference.Value.position)}
Enemy Spawn Params: Delay: {_currentSpawnDelay.Value.ToString("0.00")}
Cap: {_currentSpawnCap.Value.ToString("0.00")}
Count: {_currentEnemyCount.Value}
Seed: {_seedDebugReference.Value}";
        }

        private string formatVector3(Vector3 vector3) {
            return $"{vector3.x.ToString("0.00")}, {vector3.y.ToString("0.00")}, {vector3.z.ToString("0.00")}";
        }
    }
}