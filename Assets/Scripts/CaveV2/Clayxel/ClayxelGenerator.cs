using System.Linq;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.CaveGraph;
using Clayxels;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace CaveV2.Clayxel
{
    [ExecuteInEditMode]
    public class ClayxelGenerator : MonoBehaviour
    {
        [Required, SerializeField] private ClayContainer _clayContainer;

        [SerializeField, OnValueChanged("OnValueChangedInvertNormals")] private bool _invertNormals = false;
        private void OnValueChangedInvertNormals()
        {
            _clayContainer.setMeshInvertNormals(_invertNormals);
        }

        [SerializeField] private bool _freezeToMesh = true;

        #if UNITY_EDITOR
        [SerializeField] private bool _instanceAsPrefabs = true;
        #else
        private bool _instanceAsPrefabs = false;
        #endif

        [SerializeField] private bool _retopo = false;

        [SerializeField, OnValueChanged("OnValueChangedGenerateCollisionMesh")] private bool _generateCollisionMesh = true;
        private void OnValueChangedGenerateCollisionMesh()
        {
            if (!_generateCollisionMesh)
            {
                var meshCollider = _clayContainer.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    GameObject.DestroyImmediate(meshCollider);
                }
            }
        }

        #region Unity lifecycle

        

        #endregion

        #region Clayxels
        
        [Button, PropertyOrder(-1)]
        public void GenerateClayxels()
        {
            this.GenerateClayxels(_clayContainer, _instanceAsPrefabs);
            if (_freezeToMesh)
            {
                _clayContainer.freezeContainersHierarchyToMesh();
            }
            if (_freezeToMesh && _retopo)
            {
                _clayContainer.retopo();
            }
            if (_freezeToMesh && _generateCollisionMesh)
            {
                var meshFilter = _clayContainer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    var meshCollider = _clayContainer.GetComponent<MeshCollider>();
                    if (meshCollider == null)
                    {
                        meshCollider = _clayContainer.gameObject.AddComponent<MeshCollider>();
                    }
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }
            }
            else
            {
                var meshCollider = _clayContainer.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    GameObject.DestroyImmediate(meshCollider);
                }
            }
        }

        protected virtual void GenerateClayxels(
            ClayContainer clayContainer,
            bool instanceAsPrefabs
        ) {
            ClayxelGenerator.DestroyClayxels(clayContainer);
        }
        
        [Button, PropertyOrder(-1)]
        public void DestroyClayxels()
        {
            DestroyClayxels(_clayContainer);
        }

        private static void DestroyClayxels(ClayContainer clayContainer)
        {
            if (clayContainer.isFrozenToMesh())
            {
                clayContainer.defrostToLiveClayxels();
            }
            
            var clayObjects = Enumerable
                .Range(0, clayContainer.getNumClayObjects())
                .Select(index => clayContainer.getClayObject(index))
                .ToList();  // ToList is important here, to enumerate the list BEFORE we start removing items

            foreach (var clayObject in clayObjects)
            {
                if (!clayObject.SafeIsUnityNull())
                {
                    GameObject.DestroyImmediate(clayObject.gameObject);
                }
            }

            var meshCollider = clayContainer.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                GameObject.DestroyImmediate(meshCollider);
            }
        }

        #endregion
    }
}