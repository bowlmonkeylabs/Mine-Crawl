using System.Collections.Generic;
using AdvancedSceneManager.Models;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    [InlineEditor()]
    [CreateAssetMenu(fileName = "LevelSceneCollections", menuName = "BML/Game/LevelSceneCollections", order = 0)]
    public class LevelSceneCollections : ScriptableObject
    {
        [SerializeField] private List<SceneCollection> _levels;
        public List<SceneCollection> Levels => _levels;
    }
}
