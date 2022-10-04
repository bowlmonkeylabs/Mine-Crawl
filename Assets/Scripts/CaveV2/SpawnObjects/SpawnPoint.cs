using System;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class SpawnPoint : MonoBehaviour
    {
        #region Inspector
        
        [ShowInInspector, ReadOnly] public CaveNodeData ParentNode { get; set; }
        
        [SerializeField] [Range(0f, 1f)] private float _startingSpawnChance = 1f;
        [ShowInInspector, ReadOnly] public float SpawnChance { get; set; } = 1f;
        [ShowInInspector, ReadOnly] public float EnemySpawnWeight { get; set; } = 1f;

        // TODO fetch current object tag and display it. Show a warning/error if object is NOT tagged.
        
        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            ResetSpawnProbability();
        }
        
        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            var transformCached = this.transform;
            var checkTransforms = new Transform[] { transformCached, transformCached.parent };
            if (SelectionUtils.InSelection(checkTransforms))
            {
                var position = transformCached.position;
                Gizmos.color = Color.grey;
                Gizmos.DrawSphere(position, 0.25f);
                var style = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    normal = new GUIStyleState
                    {
                        textColor = Color.white,
                    },
                };
                Handles.Label(position, this.tag, style);
            }
            #endif
        }

        #endregion
        
        #region Public interface

        public void ResetSpawnProbability()
        {
            SpawnChance = _startingSpawnChance;

            // TODO calculate base on object parameters
        }
        
        #endregion
    }
}