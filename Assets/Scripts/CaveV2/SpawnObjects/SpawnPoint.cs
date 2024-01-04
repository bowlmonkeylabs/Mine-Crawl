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
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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

        [TitleGroup("Spawn Point Id")]
        [ShowInInspector, InfoBox("This spawn point will not be found if it does not have an id generated!", InfoMessageType.Error, "@_spawnPointIdNotSet"), PropertyOrder(-1)] 
        [TitleGroup("Spawn Point Id"), Button]
        private void GenerateSpawnPointId() {
            SpawnPointId = $"{this.name}_{System.Guid.NewGuid()}";
        }
        [SerializeField, ReadOnly] public string SpawnPointId = "";
        private bool _spawnPointIdNotSet => SpawnPointId == null || SpawnPointId == "";

        
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
        [SerializeField, ShowIf("@this._projectionBehavior != SpawnPointProjectionBehavior.None")]
        private float _projectionDistance = 45f;
        [SerializeField, ShowIf("@this._projectionBehavior != SpawnPointProjectionBehavior.None")]
        public bool RequireStableSurface = true;

        [SerializeField] private bool _doInheritParentRotation = true;
        [ShowIf("@this._projectionBehavior != SpawnPointProjectionBehavior.None")] [SerializeField] private bool _alignToProjectionVector = true;
        [ShowIf("@this._projectionBehavior != SpawnPointProjectionBehavior.None")] [SerializeField] private bool _alignToSurfaceNormal = false;
        [SerializeField] private Vector3 _rotationEulerOffset = Vector3.zero;
        [SerializeField, MinMaxSlider(-180f, 180f)] private Vector2 _randomRotationRangeAroundUpAxis;
        
        [ShowInInspector, ReadOnly] private Vector3? _projectedPosition = null; 
        [ShowInInspector, ReadOnly] private Quaternion? _projectedRotation = null; 

        [FoldoutGroup("Debug"), SerializeField] private bool _showDebugPrefab;
        [FoldoutGroup("Debug"), SerializeField] private GameObject _debugPrefab;
        [FoldoutGroup("Debug"), SerializeField] private bool _alwaysDrawGizmos = false;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] protected ICaveNodeData parentNode;
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

        #endregion

        #region Unity lifecycle

        private void OnDrawGizmos()
        {
            if (_alwaysDrawGizmos)
            {
                #if UNITY_EDITOR
            
                var transformCached = this.transform;
                var checkTransforms = new Transform[] { transformCached, transformCached.parent };
                var position = transformCached.position;
                    
                if (_projectionBehavior == SpawnPointProjectionBehavior.Randomize)
                {
                    float fovVertical = _projectionDirectionRandomnessRangeDegrees.y * 2;
                    float fovHorizontal = _projectionDirectionRandomnessRangeDegrees.x * 2;
                    float aspect = fovHorizontal / fovVertical;
                    Gizmos.color = Color.yellow;
                    Gizmos.matrix = this.transform.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.LookRotation(_projectionVector));
                    Gizmos.DrawFrustum(Vector3.zero, fovVertical, _projectionDistance, 0, aspect);
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawRay(position, this.transform.TransformDirection(_projectionVector));
                }
                
                // Project();
                if (_projectedPosition != null && _projectedPosition != position)
                {
                    Gizmos.color = Color.grey;
                    DrawDebugMesh(_projectedPosition.Value, _projectedRotation ?? Quaternion.identity);
                    Gizmos.DrawLine(position, _projectedPosition.Value);
                    
                    Gizmos.matrix = Matrix4x4.Translate(_projectedPosition.Value);
                    if (_projectedRotation.HasValue)
                    {
                        Gizmos.matrix *= Matrix4x4.Rotate(_projectedRotation.Value);
                    }
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(Vector3.zero, Vector3.right);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(Vector3.zero, Vector3.up);
                    Gizmos.matrix = Matrix4x4.identity;
                }
                else
                {
                    Gizmos.color = Color.grey;
                    DrawDebugMesh(position, Quaternion.identity);
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
                
                #endif
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
                    Gizmos.matrix = this.transform.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.LookRotation(_projectionVector));
                    Gizmos.DrawFrustum(Vector3.zero, fovVertical, _projectionDistance, 0, aspect);
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawRay(position, this.transform.TransformDirection(_projectionVector));
                }
                
                // Project();
                Project();
                if (_projectedPosition != null && _projectedPosition != position)
                {
                    Gizmos.color = Color.grey;
                    DrawDebugMesh(_projectedPosition.Value, _projectedRotation ?? Quaternion.identity);
                    Gizmos.DrawLine(position, _projectedPosition.Value);
                    
                    Gizmos.matrix = Matrix4x4.Translate(_projectedPosition.Value);
                    if (_projectedRotation.HasValue)
                    {
                        Gizmos.matrix *= Matrix4x4.Rotate(_projectedRotation.Value);
                    }
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(Vector3.zero, Vector3.right);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(Vector3.zero, Vector3.up);
                    Gizmos.matrix = Matrix4x4.identity;
                }
                else
                {
                    Gizmos.color = Color.grey;
                    DrawDebugMesh(position, Quaternion.identity);
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

        private void Reset()
        {
            if(this._spawnPointIdNotSet) {
                this.GenerateSpawnPointId();
            }
        }

        #endregion
        
        #region Public interface

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
                ? this.transform.rotation
                : Quaternion.identity);

            var randomRotationDegrees = Random.Range(_randomRotationRangeAroundUpAxis.x, _randomRotationRangeAroundUpAxis.y);
            var randomRotation = Quaternion.AngleAxis(randomRotationDegrees, Vector3.up);
            
            Vector3 projectDirection;
            Quaternion rotationOffset = Quaternion.Euler(_rotationEulerOffset);
            switch (_projectionBehavior)
            {
                default:
                case SpawnPointProjectionBehavior.None:
                    _projectedPosition = this.transform.position + (Vector3.up * spawnPosOffset);
                    _projectedRotation = baseRotation * randomRotation * rotationOffset;
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

            Vector3 hitPoint, hitNormal;
            bool hitStableSurface = SpawnObjectsUtil.GetPointTowards(
                this.transform.position, 
                projectDirection, 
                out hitPoint,
                out hitNormal,
                _projectionLayerMask,
                _projectionDistance
            );

            if (hitStableSurface)
            {
                _projectedPosition = hitPoint + (projectDirection * -spawnPosOffset);
                if (_alignToSurfaceNormal)
                {
                    var rhs = Mathf.Approximately(0f, Vector3.Dot(hitNormal, Vector3.up)) ? Vector3.right : Vector3.up;
                    var perpendicularToHitNormal = Vector3.Cross(hitNormal, rhs);
                    if (perpendicularToHitNormal != Vector3.zero)
                    {
                        _projectedRotation = Quaternion.LookRotation(perpendicularToHitNormal, hitNormal);
                    }
                    else
                    {
                        _projectedRotation = baseRotation;
                    }
                }
                else if (_alignToProjectionVector)
                {
                    _projectedRotation = Quaternion.LookRotation(this.transform.position - _projectedPosition.Value)
                                         * Quaternion.Euler(90, 0, 0);
                }
                else
                {
                    _projectedRotation = baseRotation;
                }
                _projectedRotation *= rotationOffset;
                _projectedRotation *= randomRotation;
                return (_projectedPosition, _projectedRotation);
            }

            _projectedPosition = null;
            _projectedRotation = null;
            return (_projectedPosition, _projectedRotation);
        }
        
        #endregion

        protected virtual void OnPlayerVisited(object o, EventArgs e)
        {
        }

        private void DrawDebugMesh(Vector3 position, Quaternion rotation)
        {
            Gizmos.matrix = Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation);
            
            if (_debugPrefab == null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 0.1f, 0.5f));
                Gizmos.matrix = Matrix4x4.identity;
                return;
            }
            
            List<MeshFilter> meshFilters = _debugPrefab.GetComponentsInChildren<MeshFilter>()
                .Where(m =>
                {
                    bool hasRenderer = m.TryGetComponent<Renderer>(out var renderer);
                    return hasRenderer && renderer.enabled;
                })
                .ToList();
            if (meshFilters.IsNullOrEmpty())
                return;

            foreach (var meshFilter in meshFilters)
            {
                var parentLossyScale = meshFilter.transform.lossyScale;
                parentLossyScale.Scale(meshFilter.transform.localScale.Inverse());

                var meshPosition = meshFilter.transform.localPosition;
                meshPosition.Scale(parentLossyScale);
                Gizmos.color = Color.gray;
                Gizmos.DrawMesh(
                    meshFilter.sharedMesh,
                    meshPosition, 
                    meshFilter.transform.localRotation,
                    meshFilter.transform.lossyScale
                );
            }
            
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}