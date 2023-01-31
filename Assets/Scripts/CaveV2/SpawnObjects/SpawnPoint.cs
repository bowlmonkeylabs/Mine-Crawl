using System;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class SpawnPoint : MonoBehaviour
    {
        private enum SpawnPointProjectionBehavior
        {
            None,
            Gravity,
            Vector,
        }
        
        #region Inspector

        [ShowInInspector, ReadOnly]
        public ICaveNodeData ParentNode
        {
            get => parentNode;
            set
            {
                parentNode = value;
                parentNode.onPlayerVisited += OnPlayerVisited;
                _projectedPosition = null;
            }
        }

        [SerializeField] private SpawnPointProjectionBehavior _projectionBehavior;
        [SerializeField] private LayerMask _projectionLayerMask;
        [SerializeField, ShowIf("@this._projectionBehavior == SpawnPointProjectionBehavior.Vector")] 
        private Vector3 _projectionVector = Vector3.down;
        [SerializeField] private float _projectionDistance = 45f;

        [SerializeField, ReadOnly] private Vector3? _projectedPosition = null; 
        
        [SerializeField] [Range(0f, 1f)] private float _startingSpawnChance = 1f;
        [SerializeField] private bool _disableIfPlayerVisited;
        [SerializeField, ReadOnly] public bool Occupied;
        [ShowInInspector, ReadOnly] public float SpawnChance { get; set; } = 1f;
        [ShowInInspector, ReadOnly] public float EnemySpawnWeight { get; set; } = 1f;

        private ICaveNodeData parentNode;

        // TODO fetch current object tag and display it. Show a warning/error if object is NOT tagged.
        
        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            ResetSpawnProbability();
        }

        private void OnDisable()
        {
            if (parentNode != null)
            {
                parentNode.onPlayerVisited -= OnPlayerVisited;
            }
        }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            
            var transformCached = this.transform;
            var checkTransforms = new Transform[] { transformCached, transformCached.parent };
            if (SelectionUtils.InSelection(checkTransforms))
            {
                var position = transformCached.position;
                if (_projectedPosition == null && _projectionBehavior != SpawnPointProjectionBehavior.None)
                {
                    Project();
                }
                if (_projectedPosition != null && _projectedPosition != position)
                {
                    Gizmos.color = Color.grey;
                    Gizmos.DrawSphere(_projectedPosition.Value, 0.25f);
                    Gizmos.DrawLine(position, _projectedPosition.Value);
                }
                else
                {
                    Gizmos.color = Color.grey;
                    Gizmos.DrawSphere(position, 0.25f);
                }
                
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

        [Button]
        private void UpdateGizmo()
        {
            Project();
        }
        
        public Vector3? Project(float spawnPosOffset = 0)
        {
            Vector3 projectDirection;
            switch (_projectionBehavior)
            {
                default:
                case SpawnPointProjectionBehavior.None:
                    _projectedPosition = this.transform.position + (Vector3.up * spawnPosOffset);
                    return _projectedPosition;
                case SpawnPointProjectionBehavior.Gravity:
                    projectDirection = Vector3.down;
                    break;
                case SpawnPointProjectionBehavior.Vector:
                    projectDirection = _projectionVector;
                    break;
            }

            Vector3 hitPos;
            bool hitStableSurface = SpawnObjectsUtil.GetPointTowards(
                this.transform.position, 
                projectDirection, 
                out hitPos,
                _projectionLayerMask,
                _projectionDistance
            );

            if (hitStableSurface)
            {
                _projectedPosition = hitPos + (projectDirection * -spawnPosOffset);
                return _projectedPosition;
            }

            _projectedPosition = null;
            return _projectedPosition;
        }
        
        #endregion

        private void OnPlayerVisited(object o, EventArgs e)
        {
            if (_disableIfPlayerVisited)
                SpawnChance = 0f;
        }
    }
}