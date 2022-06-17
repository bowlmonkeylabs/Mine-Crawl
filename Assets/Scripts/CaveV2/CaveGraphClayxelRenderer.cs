using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using UnityEngine;
using Clayxels;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;

namespace BML.Scripts.CaveV2
{
    [ExecuteInEditMode]
    public class CaveGraphClayxelRenderer : MonoBehaviour
    {
        [Required, SerializeField] private ClayContainer _clayContainer;
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;

        [SerializeField, OnValueChanged("UpdateInvertNormals")] private bool _invertNormals = false;
        private void UpdateInvertNormals()
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

        [SerializeField] private bool _generateCollisionMesh = true;

        [InlineEditor]
        [Required, SerializeField] private CaveGraphClayxelRendererParameters _caveGraphRenderParams;

        #region Unity lifecycle

        

        #endregion

        #region Clayxels

        [Button, PropertyOrder(-1)]
        public void GenerateClayxels()
        {
            var caveGraph = _caveGenerator.CaveGraph;
            if (caveGraph != null)
            {
                CaveGraphClayxelRenderer.GenerateClayxels(_clayContainer, caveGraph, _caveGraphRenderParams, _instanceAsPrefabs);
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
                    GameObject.DestroyImmediate(meshCollider);
                }
            }
        }

        private static void GenerateClayxels(
            ClayContainer clayContainer,
            CaveGraphV2 caveGraph,
            CaveGraphClayxelRendererParameters renderParams,
            bool instanceAsPrefabs
        ) {
            CaveGraphClayxelRenderer.DestroyClayxels(clayContainer);

            var localOrigin = clayContainer.transform.position;
            
            // Spawn "rooms" at each cave node
            foreach (var caveNodeData in caveGraph.Vertices)
            {
                // Select room to spawn
                GameObject roomPrefab;
                if (caveNodeData == caveGraph.StartNode &&
                    !renderParams.StartRoomPrefab.SafeIsUnityNull())
                {
                    roomPrefab = renderParams.StartRoomPrefab;
                }
                else
                {
                    // TODO systematically choose which rooms to spawn
                    roomPrefab = renderParams.GetRandomRoom();
                }

                // Spawn room
                GameObject newGameObject;
                if (instanceAsPrefabs)
                {
                    var newObject = PrefabUtility.InstantiatePrefab(roomPrefab, clayContainer.transform);
                    newGameObject = newObject as GameObject;
                }
                else
                {
                    newGameObject = GameObject.Instantiate(roomPrefab, clayContainer.transform);
                }
                newGameObject.transform.position = localOrigin + caveNodeData.LocalPosition;
            }

            // TODO find out how to update the Clayxels without the game object selected; none of the below seem to update it
            // clayContainer.needsUpdate = true;
            // clayContainer.scanClayObjectsHierarchy();
            // clayContainer.forceUpdateAllSolids();
            // clayContainer.solidUpdated(0);
            // clayContainer.computeClay();
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
        }

        #endregion
    }
}