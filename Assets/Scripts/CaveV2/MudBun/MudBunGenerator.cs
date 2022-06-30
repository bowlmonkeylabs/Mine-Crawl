using System;
using System.Linq;
using BML.Scripts.Utils;
using MudBun;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using MeshUtils = BML.Scripts.Utils.MeshUtils;

namespace BML.Scripts.CaveV2.MudBun
{
    [ExecuteAlways]
    public class MudBunGenerator : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private bool _generateOnChange;
        [SerializeField] private int _maxGeneratesPerSecond = 1;
        private float _generateMinCooldownSeconds => 1f / (float) _maxGeneratesPerSecond;
        
        [Required, SerializeField] private MudRenderer _mudRenderer;
        
        [SerializeField] private bool _invertNormals = false;

        #if UNITY_EDITOR
        [SerializeField] private bool _instanceAsPrefabs = true;
        #else
        private bool _instanceAsPrefabs = false;
        #endif

        #endregion
        
        #region Events

        public delegate void AfterGenerate();

        public AfterGenerate OnAfterGenerate;

        public delegate void AfterLockMesh();

        public AfterLockMesh OnAfterLockMesh;
        
        public delegate void AfterAddCollider();

        public AfterAddCollider OnAfterAddCollider;

        public delegate void AfterFinished();

        public AfterFinished OnAfterFinished;
        private bool _onAfterLockMeshTriggered, _onAfterAddColliderTriggered;
        private object _onFinishedLock = new object();
        
        #endregion
        
        #region Unity lifecycle

        protected virtual void OnEnable()
        {
            _mudRenderer.OnAfterLockMesh += OnAfterLockMeshCallback;
            _mudRenderer.OnAfterAddCollider += OnAfterAddColliderCallback;
        }
        
        protected virtual void OnDisable()
        {
            _mudRenderer.OnAfterLockMesh -= OnAfterLockMeshCallback;
            _mudRenderer.OnAfterAddCollider -= OnAfterAddColliderCallback;
        }
        
        protected virtual void OnValidate()
        {
            TryGenerateWithCooldown();
        }

        #endregion

        #region MudBun

        protected void OnAfterLockMeshCallback()
        {
            // Debug.Log($"On After Lock Mesh: Invert normals {_invertNormals}");
            // Invert mesh normals
            if (_invertNormals)
            {
                var meshFilter = _mudRenderer.GetComponent<MeshFilter>();
                var meshCollider = _mudRenderer.GetComponent<MeshCollider>();
                if (!meshFilter.SafeIsUnityNull() &&
                    (meshCollider.SafeIsUnityNull() ||
                     meshCollider.sharedMesh != meshFilter.sharedMesh))
                {
                    meshFilter.sharedMesh.InvertNormals();
                    
                    var meshTemp = meshFilter.sharedMesh;
                    meshFilter.sharedMesh = null;
                    meshFilter.sharedMesh = meshTemp;
                }
            }
            
            // Debug.Log($"On After Lock Mesh: Shadow casting mode {_mudRenderer.CastShadows}");
            if (_mudRenderer.CastShadows != ShadowCastingMode.On
                || _mudRenderer.ReceiveShadows != true)
            {
                var meshRenderer = _mudRenderer.GetComponent<MeshRenderer>();
                if (!meshRenderer.SafeIsUnityNull())
                {
                    meshRenderer.shadowCastingMode = _mudRenderer.CastShadows;
                    meshRenderer.receiveShadows = _mudRenderer.ReceiveShadows;
                }
            }
            
            OnAfterLockMesh?.Invoke();

            lock (_onFinishedLock)
            {
                if (_onAfterAddColliderTriggered)
                {
                    _onAfterLockMeshTriggered = false;
                    _onAfterAddColliderTriggered = false;
                    OnAfterFinished?.Invoke();
                }
                else
                {
                    _onAfterLockMeshTriggered = true;
                }
            }
        }

        protected void OnAfterAddColliderCallback()
        {
            // Debug.Log($"On After Add Collider: Invert normals {_invertNormals}");
            // Invert mesh normals
            if (_invertNormals)
            {
                var meshFilter = _mudRenderer.GetComponent<MeshFilter>();
                var meshCollider = _mudRenderer.GetComponent<MeshCollider>();
                // Debug.Log($"Mesh collider: {meshCollider} | {meshCollider?.name} | {meshCollider?.sharedMesh?.name}");
                if (!meshCollider.SafeIsUnityNull() &&
                    (meshFilter.SafeIsUnityNull() ||
                    meshCollider.sharedMesh != meshFilter.sharedMesh))
                {
                    meshCollider.sharedMesh.InvertNormals();

                    var meshTemp = meshCollider.sharedMesh;
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshTemp;
                }
            }

            OnAfterAddCollider?.Invoke();
            
            lock (_onFinishedLock)
            {
                if (_onAfterLockMeshTriggered)
                {
                    _onAfterLockMeshTriggered = false;
                    _onAfterAddColliderTriggered = false;
                    OnAfterFinished?.Invoke();
                }
                else
                {
                    _onAfterAddColliderTriggered = true;
                }
            }
        }
        
        private float lastGenerateTime;
        protected void TryGenerateWithCooldown()
        {
            if (_generateOnChange)
            {
                var elapsedTime = (Time.time - lastGenerateTime);
                if (elapsedTime >= _generateMinCooldownSeconds)
                {
                    lastGenerateTime = Time.time;
                    this.GenerateMudBun();
                }
            }
        }
        
        [Button, PropertyOrder(-1)]
        public void GenerateMudBun()
        {
            this.GenerateMudBun(_mudRenderer, _instanceAsPrefabs);
            
            OnAfterGenerate?.Invoke();
        }

        protected virtual void GenerateMudBun(
            MudRenderer mudRenderer,
            bool instanceAsPrefabs
        ) {
            MudBunGenerator.DestroyMudBun(mudRenderer);
        }
        
        [Button, PropertyOrder(-1)]
        public void DestroyMudBun()
        {
            DestroyMudBun(_mudRenderer);
        }

        private static void DestroyMudBun(MudRenderer mudRenderer)
        {
            // mudRenderer.DestroyAllBrushesImmediate();
            var allBrushes = mudRenderer.Brushes.ToList();
            foreach (var mudRendererBrush in allBrushes)
            {
                if (!mudRendererBrush.SafeIsUnityNull() &&
                    !mudRendererBrush.gameObject.SafeIsUnityNull())
                {
                    GameObject.DestroyImmediate(mudRendererBrush.gameObject);
                }
            }
        }

        #endregion
    }
}