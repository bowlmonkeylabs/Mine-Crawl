using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        [ShowInInspector, ReadOnly, InfoBox("This spawn point will not be found if it's untagged!", InfoMessageType.Error, "@_isUntagged"), PropertyOrder(-1)] 
        private string _tag => this.tag;
        private bool _isUntagged => _tag == null || _tag == "Untagged";

        [TitleGroup("Projection behavior")]
        [SerializeField, HideLabel] private SpawnPointProjectionBehavior _projectionBehavior;
        [SerializeField] private LayerMask _projectionLayerMask;
        [SerializeField, ShowIf("@this._projectionBehavior == SpawnPointProjectionBehavior.Vector || this._projectionBehavior == SpawnPointProjectionBehavior.Randomize")] 
        private Vector3 _projectionVector = Vector3.down;
        [SerializeField, ShowIf("@this._projectionBehavior == SpawnPointProjectionBehavior.Randomize")] 
        private Vector2 _projectionDirectionRandomnessRangeDegrees = new Vector2(30, 15);
        [SerializeField] private float _projectionDistance = 45f;

        [SerializeField] private bool _doInheritParentRotation = true;
        [SerializeField] private bool _rotateTowardsSurfaceNormal = true;
        [SerializeField] private Vector3 _rotationEulerOffset = Vector3.zero;
        
        [SerializeField, ReadOnly] private Vector3? _projectedPosition = null; 
        [SerializeField, ReadOnly] private Quaternion? _projectedRotation = null; 
        
        [TitleGroup("Spawn behavior")]
        [SerializeField] [Range(0f, 1f)] private float _startingSpawnChance = 1f;
        [SerializeField] private bool _disableIfPlayerVisited = false;
        [SerializeField, Tooltip("If the last enemy to occupy this spawn point was despawned, this will ensure they get respawned immediately when the spawn point is active again.")] 
        private bool _persistSpawns = false;
        [ShowInInspector, ReadOnly] private bool _previousSpawnWasDespawned = false;
        [SerializeField, Tooltip("Guarantees this spawn point will be populated as soon as it becomes active.")] 
        private bool _guaranteeSpawnIfInRange = false;
        [SerializeField, Tooltip("Prevents more than one enemy from spawning here at a time. No other enemy can spawn until the occupier either dies or despawns.")] 
        private bool _limitToOneSpawnAtATime = false;
        [SerializeField, Tooltip("Limit the number of enemies this spawner can produce for it's lifetime.")] 
        private int _spawnLimit = -1;
        [ShowInInspector, ReadOnly] public bool ReachedSpawnLimit => (_spawnLimit > -1 && _spawnCount >= _spawnLimit);
        [ShowInInspector, ReadOnly] private int _spawnCount = 0;
        [SerializeField, Tooltip("If enabled, the above 'spawn limit' will only be applied to spawns from 'guarantee spawn if in range'; after the limit is reached guaranteed spawns will cease, but the spawn point will still be available for the EnemySpawnManager to repopulate through it's normal spawning routine.")] 
        private bool _applySpawnLimitOnlyToGuaranteedSpawns = false;
        [FormerlySerializedAs("IgnoreGlobalSpawnCap")] [SerializeField, Tooltip("Ignore the EnemySpawnManager's global concurrent spawn limit.")] 
        private bool _ignoreGlobalSpawnCap = false;
        [ShowInInspector, ReadOnly] public bool IgnoreGlobalSpawnCap => _ignoreGlobalSpawnCap && (!OnlyIgnoreGlobalSpawnCapForGuaranteedSpawns || (_guaranteeSpawnIfInRange && (_spawnLimit <= -1 || _spawnLimit >= _spawnCount))); 
        [SerializeField] 
        public bool OnlyIgnoreGlobalSpawnCapForGuaranteedSpawns = true; 

        [FoldoutGroup("Debug"), SerializeField] private bool _showDebugPrefab;
        [FoldoutGroup("Debug"), SerializeField] private GameObject _debugPrefab;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private ICaveNodeData parentNode;
        public ICaveNodeData ParentNode
        {
            get => parentNode;
            set
            {
                parentNode = value;
                parentNode.onPlayerVisited += OnPlayerVisited;
                _projectedPosition = null;
                _projectedRotation = null;
            }
        }
        
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly] public bool Occupied;
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly]
        public float SpawnChance
        {
            get => _spawnChance * (!_applySpawnLimitOnlyToGuaranteedSpawns && _spawnLimit > -1 && _spawnCount >= _spawnLimit ? 0 : 1);
            set => _spawnChance = value;
        }
        private float _spawnChance = 1f;
        
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly] public bool SpawnImmediate => ((_guaranteeSpawnIfInRange && (_spawnLimit <= -1 || _spawnLimit > _spawnCount)) 
                                                                   || (_persistSpawns && _previousSpawnWasDespawned));
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly] public float EnemySpawnWeight { get; set; } = 1f;

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
                
                Project();
                if (_projectedPosition != null && _projectedPosition != position)
                {
                    Gizmos.color = Color.grey;
                    DrawDebugMesh(_projectedPosition.Value);
                    Gizmos.DrawLine(position, _projectedPosition.Value);
                }
                else
                {
                    Gizmos.color = Color.grey;
                    DrawDebugMesh(position);
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
                // Handles.Label(position + Vector3.down * 0.3f, this.tag, style);
                Handles.Label(position + Vector3.up * 0.3f, this.name, style);
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

        [TitleGroup("Projection behavior"), Button]
        private void TestProjection()
        {
            Project();
        }
        
        public (Vector3? position, Quaternion? rotation) Project(float spawnPosOffset = 0, int? seed = null)
        {
            Vector3 cachedPosition = this.transform.position;
            int thisSeed = (seed ?? 0) + Mathf.RoundToInt(cachedPosition.x + cachedPosition.y + cachedPosition.z);
            Random.InitState(thisSeed);
            
            var baseRotation = (_doInheritParentRotation
                ? parentNode?.GameObject?.transform.rotation ?? Quaternion.identity
                : Quaternion.identity);
            
            Vector3 projectDirection;
            Quaternion rotationOffset = Quaternion.Euler(_rotationEulerOffset);
            switch (_projectionBehavior)
            {
                default:
                case SpawnPointProjectionBehavior.None:
                    _projectedPosition = this.transform.position + (Vector3.up * spawnPosOffset);
                    _projectedRotation = baseRotation * rotationOffset;
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
                    : baseRotation;
                _projectedRotation *= rotationOffset;
                return (_projectedPosition, _projectedRotation);
            }

            _projectedPosition = null;
            _projectedRotation = null;
            return (_projectedPosition, _projectedRotation);
        }

        public void RecordEnemySpawned(bool occupySpawnPoint)
        {
            _spawnCount++;
            Occupied = (occupySpawnPoint || _limitToOneSpawnAtATime);
        }

        public void RecordEnemyDespawned()
        {
            if (_persistSpawns)
            {
                _previousSpawnWasDespawned = true;
            }
            if (_persistSpawns || _spawnLimit > -1)
            {
                _spawnCount--;
            }
            Occupied = false;
        }

        public void RecordEnemyDied()
        {
            _previousSpawnWasDespawned = false;
            Occupied = false;
        }
        
        #endregion

        private void OnPlayerVisited(object o, EventArgs e)
        {
            if (_disableIfPlayerVisited)
            {
                SpawnChance = 0f;
            }
        }

        private void DrawDebugMesh(Vector3 position)
        {
            if (_debugPrefab == null)
            {
                Gizmos.DrawSphere(position, 0.25f);
                return;
            }
            
            List<MeshFilter> meshFilters = _debugPrefab.GetComponentsInChildren<MeshFilter>().ToList();
            if (meshFilters.IsNullOrEmpty())
                return;

            Quaternion rotationOffset = Quaternion.Euler(_rotationEulerOffset);
            foreach (var meshFilter in meshFilters)
            {
                Quaternion rotation = rotationOffset * meshFilter.transform.rotation;
                
                Gizmos.DrawMesh(meshFilter.sharedMesh,
                (position + meshFilter.transform.localPosition)
                    .RotatePointAroundPivot(_projectedPosition.Value, rotationOffset), 
                rotation, 
                meshFilter.transform.lossyScale);
            }
        }
    }
}