using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.Utils;
using MudBun;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace BML.Scripts.CaveV2.MudBun
{
    [ExecuteAlways]
    public class MudBunGenerator : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private bool _generateOnChange;
        [SerializeField] private int _maxGeneratesPerSecond = 1;
        private float _generateMinCooldownSeconds => 1f / (float) _maxGeneratesPerSecond;

        [SerializeField] private bool _lockAfterGenerate = false;
        
        [SerializeField] [EnableIf("_lockAfterGenerate")] [Tooltip("Enable _lockAfterGenerate for edit mode. _lockAfterGenerate MUST BE TRUE FOR THIS TO WORK.")]
        private bool _lockInEditMode = true;
        
        [SerializeField, PropertyTooltip("Use to prevent script from automatically unlocking the Mud Renderer.")] private bool _requireManualUnlock = false;
        
        [Required, SerializeField] private MudRenderer _mudRenderer;

        [SerializeField] private bool _invertNormals = false;

#if UNITY_EDITOR
        [SerializeField] private bool _instanceAsPrefabs = true;
#else
        private bool _instanceAsPrefabs = false;
#endif

        [SerializeField] private bool _enableLogs = false;

        #endregion

        #region Events

        [SerializeField] private GameEvent _onAfterGenerate;

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
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
        }

        protected virtual void OnDisable()
        {
            _mudRenderer.OnAfterLockMesh -= OnAfterLockMeshCallback;
            _mudRenderer.OnAfterAddCollider -= OnAfterAddColliderCallback;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#endif
        }

        protected virtual void OnValidate()
        {
            TryGenerateWithCooldown_OnValidate();
        }
        
        private bool _isGenerateOnChangeEnabled = false;
#if UNITY_EDITOR
        private void PlayModeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    _isGenerateOnChangeEnabled = true;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    _isGenerateOnChangeEnabled = false;
                    break;
            }
            if (_enableLogs) Debug.Log($"MudBun: Play Mode State Changed: {this.name} {stateChange} {_isGenerateOnChangeEnabled}");
        }
#endif

        #endregion

        #region MudBun

        public bool IsMeshLocked => _mudRenderer.MeshLocked;
        private bool IsMudBunGenerationEnabled => !_requireManualUnlock || !_mudRenderer.MeshLocked; // Don't let the IDE fool you!; this is not referenced in C# code, but referenced by name in a string passed to Odin attributes.

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

        protected virtual void TryGenerateWithCooldown()
        {
            if (_enableLogs) Debug.Log($"MudBun: Try generate with cooldown");
            if (_generateOnChange && !_mudRenderer.MeshLocked)
            {
                var elapsedTime = (Time.time - lastGenerateTime);
                if (elapsedTime >= _generateMinCooldownSeconds || ApplicationUtils.IsPlaying_EditorSafe)
                {
                    if (_enableLogs) Debug.Log($"MudBun: Generating");
                    lastGenerateTime = Time.time;
                    this.GenerateMudBunInternal();
                }
                else
                {
                    if (_enableLogs) Debug.Log($"MudBun: Destroying");
                    DestroyMudBun();
                }
            }
        }

        protected void TryGenerateWithCooldown_OnValidate()
        {
            if (ApplicationUtils.IsPlaying_EditorSafe || !_isGenerateOnChangeEnabled) return;

            TryGenerateWithCooldown();
        }

        [Button("Generate Mud Bun"), PropertyOrder(-1), EnableIf("IsMudBunGenerationEnabled")]
        protected virtual void GenerateMudBunInternal()
        {
            if (_enableLogs) Debug.Log($"MudBun: Generate MudBun");
            if (_mudRenderer.MeshLocked)
            {
                if (_requireManualUnlock) return;
                
                this.UnlockMesh();
            }
            
            this.GenerateMudBunInternal(_mudRenderer, _instanceAsPrefabs);
            
            OnAfterGenerate?.Invoke();
            _onAfterGenerate.Raise();

            if (_lockAfterGenerate)
            {
                if (_lockInEditMode
                    || ApplicationUtils.IsPlaying_EditorSafe)
                {
                    if (_enableLogs) Debug.Log($"MudBun: Lock after generate");
                    this.LockMesh();
                }
            }
        }

        protected virtual void GenerateMudBunInternal(
            MudRenderer mudRenderer,
            bool instanceAsPrefabs
        )
        {
            this.DestroyMudBun(mudRenderer);
        }
        
        [Button, PropertyOrder(-1), EnableIf("IsMudBunGenerationEnabled")]
        public void DestroyMudBun()
        {
            DestroyMudBun(_mudRenderer);
        }

        private void DestroyMudBun(MudRenderer mudRenderer)
        {
            if (_enableLogs) Debug.Log($"MudBun: Destroy MudBun");
            if (mudRenderer.MeshLocked)
            {
                if (_requireManualUnlock) return;
                
                if (_enableLogs) Debug.Log($"MudBun: Unlock mesh");
                this.UnlockMesh();
            }

            // mudRenderer.DestroyAllBrushesImmediate();
            mudRenderer.RescanBrushesImmediate();
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

        [Button, PropertyOrder(-1), DisableIf("$IsMeshLocked")]
        public void LockMesh()
        {
            DoLockMesh(this.transform, _mudRenderer.RecursiveLockMeshByEditor);
        }

        [Button, PropertyOrder(-1), EnableIf("$IsMeshLocked")]
        public void UnlockMesh()
        {
            if (_enableLogs) Debug.Log($"MudBun: Unlock mesh");
            _mudRenderer.UnlockMesh();
        }
        
        protected void DoLockMesh(Transform t, bool recursive, int depth = 0)
        {
            if (t == null)
                return;
            
            _mudRenderer.MeshGenerationLockOnStartByEditor = true;

            if (recursive)
            {
                for (int i = 0; i < t.childCount; ++i)
                    DoLockMesh(t.GetChild(i), recursive, depth + 1);
            }

            var renderer = t.GetComponent<MudRenderer>();
            if (renderer == null)
                return;

            bool createNewObject = _mudRenderer.MeshGenerationCreateNewObject;
            bool autoRigging = _mudRenderer.MeshGenerationAutoRigging;
            bool generateCollider = _mudRenderer.MeshGenerationCreateCollider;
            bool generateColliderMeshAsset = _mudRenderer.GenerateColliderMeshAssetByEditor;
            bool generateMeshAsset = _mudRenderer.GenerateMeshAssetByEditor;

            var prevMeshGenerationRenderableMeshMode = renderer.MeshGenerationRenderableMeshMode;
            if (createNewObject)
                renderer.MeshGenerationRenderableMeshMode = MudRendererBase.RenderableMeshMode.MeshRenderer;

            bool optimizeMeshForRendering =
                !_mudRenderer
                    .GenerateMeshAssetByEditor; // generated mesh assets are automatically optimized by the import pipeline
            renderer.LockMesh(autoRigging,
                false,
                null,
                _mudRenderer.MeshGenerationGenerateTextureUV,
                _mudRenderer.MeshGenerationGenerateLightMapUV,
                _mudRenderer.MeshGenerationWeldVertices,
                optimizeMeshForRendering
            );

            if (createNewObject)
                renderer.MeshGenerationRenderableMeshMode = prevMeshGenerationRenderableMeshMode;

            // finish all access to serialized properties before they get disposed upon asset database refresh

            if (generateCollider)
            {
                var colliderMesh = renderer.AddCollider(renderer.gameObject, false, null,
                    _mudRenderer.MeshGenerationForceConvexCollider,
                    _mudRenderer.MeshGenerationCreateRigidBody
                );
                if (colliderMesh != null
                    && generateColliderMeshAsset)
                {
#if UNITY_EDITOR
                    renderer.ValidateAssetNames();

                    string rootFolder = "Assets";
                    string assetsFolder = "MudBun Generated Assets";
                    string folderPath = $"{rootFolder}/{assetsFolder}";
                    string assetName = renderer.GenerateColliderMeshAssetByEditorName;

                    if (!AssetDatabase.IsValidFolder(folderPath))
                        AssetDatabase.CreateFolder(rootFolder, assetsFolder);

                    string meshAssetPath = $"{folderPath}/{assetName}.mesh";
                    AssetDatabase.CreateAsset(colliderMesh, meshAssetPath);
                    AssetDatabase.Refresh();

                    Debug.Log($"MudBun: Saved collider mesh asset - \"{folderPath}/{assetName}.mesh\"");
#else
                    Debug.LogWarning("Unable to generate collider asset in build! Requires access to the UnityEditor.AssetDatabase namespace.");
#endif
                }
            }
            
            if (generateMeshAsset)
            {
#if UNITY_EDITOR
                renderer.ValidateAssetNames();

                string rootFolder = "Assets";
                string assetsFolder = "MudBun Generated Assets";
                string folderPath = $"{rootFolder}/{assetsFolder}";
                string assetName = renderer.GenerateMeshAssetByEditorName;

                if (!AssetDatabase.IsValidFolder(folderPath))
                    AssetDatabase.CreateFolder(rootFolder, assetsFolder);

                Mesh mesh = null;
                Material mat = null;
                var meshFilter = renderer.GetComponent<MeshFilter>();
                var meshRenderer = renderer.GetComponent<MeshRenderer>();
                var skinnedMeshRenderer = renderer.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    if (meshFilter != null)
                    {
                        mesh = meshFilter.sharedMesh;
                    }

                    mat = meshRenderer.sharedMaterial;
                }
                else if (skinnedMeshRenderer != null)
                {
                    mesh = skinnedMeshRenderer.sharedMesh;
                    mat = skinnedMeshRenderer.sharedMaterial;
                }

                if (mesh != null)
                {
                    string meshAssetPath = $"{folderPath}/{assetName}.mesh";
                    AssetDatabase.CreateAsset(mesh, meshAssetPath);
                    AssetDatabase.Refresh();

                    Debug.Log($"MudBun: Saved mesh asset - \"{folderPath}/{assetName}.mesh\"");

                    // somehow serialized properties get invalidated after asset database operations
                    // InitSerializedProperties();

                    var savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
                    if (savedMesh != null)
                    {
                        if (meshFilter != null)
                            meshFilter.sharedMesh = savedMesh;
                    }
                }

                if (mat != null)
                {
                    if (meshRenderer != null)
                        meshRenderer.sharedMaterial = mat;
                    else if (skinnedMeshRenderer != null)
                        skinnedMeshRenderer.sharedMaterial = mat;
                }
#else
                Debug.LogWarning("Unable to generate mesh asset in build! Requires access to the UnityEditor.AssetDatabase namespace.");
#endif
            }

            if (depth == 0
                && createNewObject)
            {
                var clone = Instantiate(renderer.gameObject);
                clone.name = renderer.name + " (Locked Mesh Clone)";

                if (autoRigging)
                {
                    var cloneRenderer = clone.GetComponent<MudRenderer>();
                    cloneRenderer.RescanBrushesImmediate();
                    cloneRenderer.DestroyAllBrushesImmediate();
                }
                else
                {
                    DestroyAllChildren(clone.transform);
                }
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(clone, clone.name);
#endif
                DestroyImmediate(clone.GetComponent<MudRenderer>());
#if UNITY_EDITOR
                Selection.activeObject = clone;
#endif

                renderer.UnlockMesh();
            }
        }
        
        protected void DestroyAllChildren(Transform t, bool isRoot = true)
        {
            if (t == null)
                return;

            var aChild = new Transform[t.childCount];
            for (int i = 0; i < t.childCount; ++i)
                aChild[i] = t.GetChild(i);
            foreach (var child in aChild)
                DestroyAllChildren(child, false);

            if (!isRoot)
                DestroyImmediate(t.gameObject);
        }

        #endregion
    }
}