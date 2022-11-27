using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        public static Vector2Int TorchRequirementMinMax = new Vector2Int(2, 8);
        private static string _addressTorchAreaCoverage = @"Assets/Entities/CaveGen/Cave_TorchAreaCoverage.asset";
        
        public static float GetTorchAreaCoverage()
        {
            var asyncHandle = Addressables.LoadAssetAsync<FloatVariable>(_addressTorchAreaCoverage);
            asyncHandle.WaitForCompletion();
            return asyncHandle.Result.Value;
        }

        public static int CalculateTorchRequirement(this CaveNodeData caveNodeData)
        {
            if (caveNodeData.BoundsColliders.Count == 0)
            {
                return TorchRequirementMinMax.x;
            } 
            
            var approxBoundsAreas = caveNodeData.BoundsColliders.Select(collider =>
            {
                Vector3 boundsWorldSize = Vector3.Scale(collider.bounds.size, collider.transform.lossyScale);
                return boundsWorldSize.x * boundsWorldSize.z;
            });
            float nodeApproxArea = approxBoundsAreas.Max();
            
            float torchRequirement = nodeApproxArea / GetTorchAreaCoverage();
            int rounded = Mathf.Clamp(Mathf.RoundToInt(torchRequirement), TorchRequirementMinMax.x, TorchRequirementMinMax.y);
            return rounded;
        }
    }
}