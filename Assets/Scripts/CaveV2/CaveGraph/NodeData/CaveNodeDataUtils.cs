using System.Collections.Generic;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.NodeData
{
    public static class CaveNodeDataUtils
    {
        public static LayerMask RoomBoundsLayerMask => LayerMask.GetMask("RoomBounds");
        
        public static List<Collider> GetRoomBoundsColliders(this ICaveNodeData caveNodeData, LayerMask roomBoundsLayerMask)
        {
            List<Collider> boundsColliders = new List<Collider>();

            Collider[] allRoomColliders = caveNodeData.GameObject.GetComponentsInChildren<Collider>();
            foreach (var col in allRoomColliders)
            {
                if (col.gameObject.IsInLayerMask(roomBoundsLayerMask))
                {
                    boundsColliders.Add(col);
                }
            }

            return boundsColliders;
        }
    }
}