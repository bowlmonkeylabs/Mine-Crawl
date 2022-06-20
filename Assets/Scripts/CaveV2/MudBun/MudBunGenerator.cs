using System;
using System.Linq;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using Clayxels;
using MudBun;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using MeshUtils = BML.Scripts.Utils.MeshUtils;

namespace CaveV2.MudBun
{
    [ExecuteInEditMode]
    public class MudBunGenerator : MonoBehaviour
    {
        [Required, SerializeField] private MudRenderer _mudRenderer;
        
        [SerializeField] private bool _invertNormals = false;

#if UNITY_EDITOR
        [SerializeField] private bool _instanceAsPrefabs = true;
        #else
        private bool _instanceAsPrefabs = false;
        #endif

        #region Unity lifecycle

        private void OnEnable()
        {
            _mudRenderer.OnAfterLockMesh += OnAfterLockMesh;
            _mudRenderer.OnAfterAddCollider += OnAfterAddCollider;
        }
        
        private void OnDisable()
        {
            _mudRenderer.OnAfterLockMesh -= OnAfterLockMesh;
            _mudRenderer.OnAfterAddCollider -= OnAfterAddCollider;
        }

        #endregion

        #region MudBun

        protected void OnAfterLockMesh()
        {
            Debug.Log($"On After Lock Mesh: Invert normals {_invertNormals}");
            // Invert mesh normals
            if (_invertNormals)
            {
                var meshFilter = _mudRenderer.GetComponent<MeshFilter>();
                if (!meshFilter.SafeIsUnityNull())
                {
                    Debug.Log($"On After Lock Mesh: Invert Mesh Filter");
                    meshFilter.sharedMesh.InvertNormals();
                }
            }
        }

        protected void OnAfterAddCollider()
        {
            Debug.Log($"On After Add Collider: Invert normals {_invertNormals}");
            // Invert mesh normals
            if (_invertNormals)
            {
                var meshFilter = _mudRenderer.GetComponent<MeshFilter>();
                var meshCollider = _mudRenderer.GetComponent<MeshCollider>();
                // Debug.Log($"Mesh collider: {meshCollider} | {meshCollider?.name} | {meshCollider?.sharedMesh?.name}");
                if (!meshCollider.SafeIsUnityNull() &&
                    meshCollider.sharedMesh != meshFilter.sharedMesh)
                {
                    Debug.Log($"On After Add Collider: Invert Mesh Collider");
                    meshCollider.sharedMesh.InvertNormals();
                }
            }
        }
        
        [Button, PropertyOrder(-1)]
        public void GenerateMudBun()
        {
            this.GenerateMudBun(_mudRenderer, _instanceAsPrefabs);
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