using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using UnityEngine;

namespace BML.Scripts.WormBait
{
    public class WormBait : MonoBehaviour
    {
        [SerializeField] private GameObjectSceneReference _enemySpawnManagerRef;

        private CaveWormSpawner caveWormSpawner;
        private void Start()
        {
            caveWormSpawner = _enemySpawnManagerRef.Value.GetComponent<CaveWormSpawner>();
            caveWormSpawner.RegisterWormBait(this);
        }
    }
}