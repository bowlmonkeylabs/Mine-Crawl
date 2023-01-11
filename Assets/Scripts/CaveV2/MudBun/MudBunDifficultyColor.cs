using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2.CaveGraph;
using MudBun;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.MudBun
{
    [ExecuteAlways]
    public class MudBunDifficultyColor : MonoBehaviour
    {
        [SerializeField] private bool _isEdge;
        [SerializeField, HideIf("_isEdge")] private CaveNodeDataDebugComponent _caveNodeDataDebug;
        [SerializeField, ShowIf("_isEdge")] private CaveNodeConnectionDataDebugComponent _caveNodeEdgeDataDebug;
        [SerializeField] private GameEvent _onAfterGenerateMudBun;
        [SerializeField] private DifficultyColorParams _difficultyColorParams;

        private void OnEnable()
        {
            _onAfterGenerateMudBun.Subscribe(SetDifficultyColor);
        }

        private void OnDisable()
        {
            _onAfterGenerateMudBun.Unsubscribe(SetDifficultyColor);
        }

        private void SetDifficultyColor()
        {
            var difficulty = _isEdge ? 
                _caveNodeEdgeDataDebug.CaveNodeConnectionData.Difficulty : _caveNodeDataDebug.CaveNodeData.Difficulty;

            var difficultyColorList = _difficultyColorParams.DifficultyColorList;
            var colorIndex = Mathf.Min(difficulty, _difficultyColorParams.DifficultyColorList.Count - 1);
            var difficultyColor = difficultyColorList[colorIndex];

            List<MudMaterial> materials = GetComponentsInChildren<MudMaterial>()?.ToList();
            materials?.ForEach(m =>
            {
                m.Color *= difficultyColor;
            });
        }
    }
}