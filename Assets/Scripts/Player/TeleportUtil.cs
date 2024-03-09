using System;
using System.Linq;
using UnityEngine;

namespace BML.Scripts.Player
{
    public static class TeleportUtil
    {
        private static string TELEPORT_TAG = "Dev_Teleport";
        private static string TELEPORT_NAME_PREFIX = "TP_";
        
        // In order to be found, teleport points should be tagged with 'Dev_Teleport' and should be named with the convention '{TP_<name>}'
        public static Transform FindTpPoint(string tpPointName)
        {
            var taggedTeleportPoints = GameObject.FindGameObjectsWithTag(TELEPORT_TAG);
            var requested = taggedTeleportPoints.FirstOrDefault(go =>
            {
                var goTrimmedName = go.name.Trim('{', '}').Remove(0, TELEPORT_NAME_PREFIX.Length);
                return goTrimmedName.Equals(tpPointName, StringComparison.OrdinalIgnoreCase);
            });

            return requested?.transform;
        }
    }
}