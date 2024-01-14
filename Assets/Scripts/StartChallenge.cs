using System;
using System.Collections;
using System.Collections.Generic;
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
        
        public void DoChallengeStart() {
            _caveNode.CaveNode.GameObject.GetComponent<ChallengeRoom>().StartChallenge();
        }
    }
}
