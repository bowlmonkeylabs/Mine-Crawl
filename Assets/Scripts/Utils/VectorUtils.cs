﻿using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class VectorUtils
    {
        #region Swizzles
        
        public static Vector3 xoy(this Vector2 vec) { return new Vector3(vec.x, 0, vec.y); }

        public static Vector2 xz(this Vector3 vec) { return new Vector2(vec.x, vec.z); }
        
        public static Vector2 xy(this Vector3 vec) { return new Vector2(vec.x, vec.y); }
        public static Vector3 xyo(this Vector3 vec) { return new Vector3(vec.x, vec.y, 0); }
        public static Vector3 xoo(this Vector3 vec) { return new Vector3(vec.x, 0, 0); }
        public static Vector3 oyo(this Vector3 vec) { return new Vector3(0, vec.y, 0); }
        public static Vector3 ooz(this Vector3 vec) { return new Vector3(0, 0, vec.z); }
        public static Vector3 xoz(this Vector3 vec) { return new Vector3(vec.x, 0f, vec.z); }
        public static Vector3 oyz(this Vector3 vec) { return new Vector3(0, vec.y, vec.z); }
    
        public static Vector2 xy(this Vector4 vec) { return new Vector2(vec.x, vec.y); }
        public static Vector2 xz(this Vector4 vec) { return new Vector2(vec.x, vec.z); }
        
        #endregion

        #region Vector3Axes Extensions

        private static Dictionary<Vector3Axes, Vector3> _axes = new Dictionary<Vector3Axes, Vector3>
        {
            {Vector3Axes.X, Vector3.right},
            {Vector3Axes.Y, Vector3.up},
            {Vector3Axes.Z, Vector3.forward},
        };
            
        public static Vector3 GetAxis(this Vector3Axes axis)
        {
            return _axes[axis];
        }
        
        #endregion
        
        #region Distance
        
        public static float SqrDistance(this Vector3 point1, Vector3 point2)
        {
            return Mathf.Pow(point2.x - point1.x, 2) +
                   Mathf.Pow(point2.y - point1.y, 2) +
                   Mathf.Pow(point2.z - point1.z, 2);
        }

        public static float SqrDistanceMasked(this Vector3 point1, Vector3 point2, bool includeX = true,
            bool includeY = true, bool includeZ = true)
        {
            return (includeX ? Mathf.Pow(point2.x - point1.x, 2) : 0) +
                   (includeY ? Mathf.Pow(point2.y - point1.y, 2) : 0) +
                   (includeZ ? Mathf.Pow(point2.z - point1.z, 2) : 0);
        }
        
        public static float SqrDistanceIgnoreY(this Vector3 point1, Vector3 point2)
        {
            return Mathf.Pow(point2.x - point1.x, 2) +
                   Mathf.Pow(point2.z - point1.z, 2);
        }

        public static bool RoughDistanceThresholdCheck(this Vector3 point1, Vector3 point2, float distanceThreshold)
        {
            var diff = (point2 - point1).Abs();
            return (diff.x <= distanceThreshold) && 
                   (diff.y <= distanceThreshold) && 
                   (diff.z <= distanceThreshold);
        }
        
        #endregion

        public static Vector2 RoundNearZero(this Vector2 vec)
        {
            var ret = new Vector2(vec.x, vec.y);
            ret.x = ret.x.RoundNearZero();
            ret.y = ret.y.RoundNearZero();
            return ret;
        }
        
        public static Vector3 RoundNearZero(this Vector3 vec)
        {
            var ret = new Vector3(vec.x, vec.y, vec.z);
            ret.x = ret.x.RoundNearZero();
            ret.y = ret.y.RoundNearZero();
            ret.z = ret.z.RoundNearZero();
            return ret;
        }

        public static Vector3Int FloorToInt(this Vector3 vec)
        {
            var ret = new Vector3Int();
            ret.x = Mathf.FloorToInt(vec.x);
            ret.y = Mathf.FloorToInt(vec.y);
            ret.z = Mathf.FloorToInt(vec.z);
            return ret;
        }
        
        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation) {
            var dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }

        /// <summary>
        /// Inverts a scale vector by dividing 1 by each component
        /// </summary>
        public static Vector3 Inverse(this Vector3 vec)
        {
            return new Vector3(1/vec.x, 1/vec.y, 1/vec.z);
        }
        
        /// <summary>
        /// Inverts a scale vector by dividing 1 by each component
        /// </summary>
        public static Vector2 Inverse(this Vector2 vec)
        {
            return new Vector2(1/vec.x, 1/vec.y);
        }

        public static Vector3 ProjectComponents(this Vector3 vec, Vector3 onNormal)
        {
            return   Vector3.Project(vec.xoo(), onNormal).Abs()
                   + Vector3.Project(vec.oyo(), onNormal).Abs()
                   + Vector3.Project(vec.ooz(), onNormal).Abs();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>Return angle in Degrees</returns>
        public static float AngleOnUnitCircle(this Vector2 vec)
        {
            return Vector2.SignedAngle(Vector2.right, vec);
        }

        public static Vector2 GetRelative(this Vector2 v1, Vector2 v2)
        {
            Quaternion rotationOffset = Quaternion.FromToRotation(Vector2.up, v1);
            Vector2 v2Relative = rotationOffset * v2;
            return v2Relative;
        }
        
        /// <summary>
        /// Different than ProjectOnPlane. This method is like looking at the vector from top-down
        /// view and pushing it directly downwards onto the slope. Useful to get move input on slope that
        /// feels better than Vector3.ProjectOnPlane
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="slopeNormal"></param>
        /// <returns></returns>
        public static Vector3 FlattenDirectionOntoSlope(Vector3 dir, Vector3 slopeNormal)
        {
            //The idea here is to create a plane from hitDir and Vector3.up
            //The hitDir flattened onto the slope (desired value) is nothing more than the line of intersection
            //between the slope plane and this newly created plane
            //To find the line of intersection of 2 planes, you just take the cross product of their normals
            Vector3 crossPlaneNormal = Vector3.Cross(Vector3.up, dir);
            Vector3 flattenedDirection = Vector3.Cross(crossPlaneNormal, slopeNormal).normalized;
            
            return flattenedDirection;
        }

        public static float Max(this Vector3 v)
        {
            return Mathf.Max(v.x, v.y, v.z);
        }

        public static Vector2 Rotate(this Vector2 vector, float degrees)
        {
            return new Vector2(
                vector.x * Mathf.Cos(degrees * Mathf.Deg2Rad) - vector.y * Mathf.Sin(degrees * Mathf.Deg2Rad),
                vector.x * Mathf.Sin(degrees * Mathf.Deg2Rad) + vector.y * Mathf.Cos(degrees * Mathf.Deg2Rad)
            );
        }

        public static string ToPrint(this Vector2 vector)
        {
            return $"({vector.x}, {vector.y})";
        }

        public static string ToPrint(this Vector3 vector)
        {
            return $"({vector.x}, {vector.y}, {vector.z})";
        }
    }

    [Serializable]
    public enum Vector3Axes
    {
        X, Y, Z
    }
}