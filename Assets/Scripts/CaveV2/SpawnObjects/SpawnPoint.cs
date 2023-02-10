using System;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class SpawnPoint : MonoBehaviour
    {
        private enum SpawnPointProjectionBehavior
        {
            None,
            Gravity,
            Vector,
            Randomize,
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
        [SerializeField, ShowIf("@this._projectionBehavior == SpawnPointProjectionBehavior.Vector || this._projectionBehavior == SpawnPointProjectionBehavior.Randomize")] 
        private Vector3 _projectionVector = Vector3.down;
        [SerializeField, ShowIf("@this._projectionBehavior == SpawnPointProjectionBehavior.Randomize")] private Vector2 _projectionDirectionRandomnessRangeDegrees = new Vector2(30, 15);
        [SerializeField] private float _projectionDistance = 45f;

        [SerializeField] private bool _rotateTowardsSurfaceNormal = true;
        [SerializeField, ReadOnly] private Vector3? _projectedPosition = null; 
        [SerializeField, ReadOnly] private Quaternion? _projectedRotation = null; 
        
        [SerializeField] [Range(0f, 1f)] private float _startingSpawnChance = 1f;
        [SerializeField] private bool _disableIfPlayerVisited;
        [SerializeField, ReadOnly] public bool Occupied;
        [ShowInInspector, ReadOnly] public float SpawnChance { get; set; } = 1f;
        [ShowInInspector, ReadOnly] public float EnemySpawnWeight { get; set; } = 1f;

        private ICaveNodeData parentNode;

        // TODO fetch current object tag and display it. Show a warning/error if object is NOT tagged.
        [ShowInInspector, ReadOnly, InfoBox("", InfoMessageType.Error, "@this.tag")] 
        private string _tag => this.tag;
        
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
                
                if (_projectionBehavior == SpawnPointProjectionBehavior.Randomize)
                {
                    float fovVertical = _projectionDirectionRandomnessRangeDegrees.y * 2;
                    float fovHorizontal = _projectionDirectionRandomnessRangeDegrees.x * 2;
                    float aspect = fovHorizontal / fovVertical;
                    Gizmos.color = Color.yellow;
                    Gizmos.matrix = this.transform.localToWorldMatrix;
                    Gizmos.DrawFrustum(Vector3.zero, fovVertical, _projectionDistance, 0, aspect);
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawRay(position, this.transform.TransformDirection(_projectionVector));
                }
                
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
        private void Test()
        {
            Project();
        }
        
        public (Vector3? position, Quaternion? rotation) Project(float spawnPosOffset = 0, int? seed = null)
        {
            Vector3 cachedPosition = this.transform.position;
            int thisSeed = (seed ?? 0) + Mathf.RoundToInt(cachedPosition.x + cachedPosition.y + cachedPosition.z);
            Random.InitState(thisSeed);
            
            Vector3 projectDirection;
            switch (_projectionBehavior)
            {
                default:
                case SpawnPointProjectionBehavior.None:
                    _projectedPosition = this.transform.position + (Vector3.up * spawnPosOffset);
                    _projectedRotation = Quaternion.identity;
                    return (_projectedPosition, _projectedRotation);
                case SpawnPointProjectionBehavior.Gravity:
                    projectDirection = Vector3.down;
                    break;
                case SpawnPointProjectionBehavior.Vector:
                    // convert to local direction
                    projectDirection = this.transform.TransformDirection(_projectionVector);
                    break;
                case SpawnPointProjectionBehavior.Randomize:
                    float randomDegreesX = Random.Range(-_projectionDirectionRandomnessRangeDegrees.x,
                        _projectionDirectionRandomnessRangeDegrees.x);
                    float randomDegreesY = Random.Range(-_projectionDirectionRandomnessRangeDegrees.y,
                        _projectionDirectionRandomnessRangeDegrees.y);
                    Quaternion rotation = Quaternion.Euler(randomDegreesY, randomDegreesX, 0); // This switch is intentional
                    projectDirection = rotation * this.transform.TransformDirection(_projectionVector);
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
                _projectedRotation = _rotateTowardsSurfaceNormal
                    ? Quaternion.LookRotation(this.transform.position - _projectedPosition.Value) *
                      Quaternion.Euler(90, 0, 0)
                    : Quaternion.identity;
                return (_projectedPosition, _projectedRotation);
            }

            _projectedPosition = null;
            _projectedRotation = null;
            return (_projectedPosition, _projectedRotation);
        }
        
        #endregion

        private void OnPlayerVisited(object o, EventArgs e)
        {
            if (_disableIfPlayerVisited)
                SpawnChance = 0f;
        }
    }
}