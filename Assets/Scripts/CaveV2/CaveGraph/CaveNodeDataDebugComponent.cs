using System;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph
{
    public class CaveNodeDataDebugComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeData CaveNodeData;
        [ReadOnly, ShowInInspector] private ShapeRenderer _renderer;
        private Color _startingColor;

        #region Unity lifecycle

        private void Awake()
        {
            _renderer = GetComponent<ShapeRenderer>();
            if (_renderer != null)
            {
                _startingColor = _renderer.Color;
            }
        }

        private void Update()
        {
            UpdatePlayerOccupied();
        }

        #endregion

        private void UpdatePlayerOccupied()
        {
            // Debug.Log($"CaveNodeDataDebugComponent: UpdatePlayerOccupied");

            if (_renderer != null)
            {
                var setColor = CaveNodeData.PlayerOccupied
                    ? Color.blue
                    : _startingColor;
                setColor.a = CaveNodeData.PlayerVisited
                    ? 0.5f
                    : _startingColor.a;
                _renderer.Color = setColor;
            }
        }
    }
}