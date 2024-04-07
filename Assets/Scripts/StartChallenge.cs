using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class StartChallenge : MonoBehaviour
    {
        [SerializeField] private SpawnedObjectCaveNodeData _caveNode;
        [SerializeField] private GameObjectSceneReference _enemySpawnManagerRef;
        
        public void DoChallengeStart() {
            //trigger specific effects for challenge room (activating spawn points, raising rocks, etc.)
            _caveNode.CaveNode.GameObject.GetComponent<ChallengeRoom>().StartChallenge();
            //activate any unactive spawn points for challenge room
            _enemySpawnManagerRef.Value.GetComponent<EnemySpawnManager>().CacheActiveSpawnPoints();
        }
    }
}
