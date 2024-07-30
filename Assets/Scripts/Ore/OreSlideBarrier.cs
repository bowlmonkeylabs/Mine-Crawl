using System;
using System.Collections;
using System.Collections.Generic;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.SpawnObjects;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class OreSlideBarrier : MonoBehaviour
    {
        [SerializeField] private MMF_Player _slideFeedback;
        [SerializeField] private SpawnedObjectCaveNodeData _spawnedObjectCaveNodeData;

        [ShowInInspector] private ChallengeRoom _challengeRoom;

        void Start()
        {
            _slideFeedback.GetFeedbackOfType<MMF_Position>().DestinationPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            transform.position = new Vector3(transform.position.x, transform.position.y - 5, transform.position.z);

            var caveNodeConnectionData = _spawnedObjectCaveNodeData.CaveNode as CaveNodeConnectionData;
            var sourceIsChallengeOrBoss = caveNodeConnectionData.Source.NodeType == CaveNodeType.Challenge || caveNodeConnectionData.Source.NodeType == CaveNodeType.Boss;
            var challegeRoomCaveNode = sourceIsChallengeOrBoss ? caveNodeConnectionData.Source : caveNodeConnectionData.Target;
            _challengeRoom = challegeRoomCaveNode.GameObject.GetComponent<ChallengeRoom>();
            _challengeRoom._onChallengeStarted += OnChallengeStarted;
        }

        void OnDisable() {
            _challengeRoom._onChallengeStarted -= OnChallengeStarted;
        }

        private void OnChallengeStarted(object o, EventArgs e) {
            _slideFeedback.PlayFeedbacks();
        }
    }
}
