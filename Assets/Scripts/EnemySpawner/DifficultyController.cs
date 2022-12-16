using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using UnityEngine;

namespace BML.Scripts
{
    public class DifficultyController : MonoBehaviour
    {
        [SerializeField] private IntVariable _currentDifficulty;
        [SerializeField] private InfluenceStateData _playerInfluenceState;
        [SerializeField] private float _updateInterval = 1f;

        private float lastUpdateTime = Mathf.NegativeInfinity;

        private void Update()
        {
            if (lastUpdateTime + _updateInterval > Time.time)
                return;
            
            UpdateDifficultyParams();
            lastUpdateTime = Time.time;
        }

        private void UpdateDifficultyParams()
        {
            bool isPlayerInNode = _playerInfluenceState._currentNodes.Count > 0;
            bool isPlayerInNodeConnection = _playerInfluenceState._currentNodeConnections.Count > 0;
            
            // Dont update parameters if player is not within node or edge
            if (!isPlayerInNode && !isPlayerInNodeConnection)
                return;

            // Aggregate the difficulty of nodes and edges player is currently occupying
            List<int> aggregateDifficulties = new List<int>();

            if (isPlayerInNode)
                aggregateDifficulties = aggregateDifficulties
                    .Union(_playerInfluenceState._currentNodes
                        .Select(n => n.Value.Difficulty)
                    ).ToList();
            
            if (isPlayerInNodeConnection)
                aggregateDifficulties = aggregateDifficulties
                    .Union(_playerInfluenceState._currentNodeConnections
                        .Select(n => n.Value.Difficulty)
                    ).ToList();

            float aggregateDifficultyFactor = (float) aggregateDifficulties.Average();

            _currentDifficulty.Value = Mathf.CeilToInt(aggregateDifficultyFactor);
        }
    }
}