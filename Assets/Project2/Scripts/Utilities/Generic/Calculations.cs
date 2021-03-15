using System;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.Custom_Interface;

namespace XR_Prototyping.Scripts.Utilities.Generic
{
    public static class Calculations
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="maxAngle"></param>
        /// <param name="minAngle"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static float CalculateDepth(this float angle, float maxAngle, float minAngle, float max, float min, Transform proxy)
        {
            float a = angle;

            a = a > maxAngle ? maxAngle : a;
            a = a < minAngle ? minAngle : a;
            
            float proportion = Mathf.InverseLerp(maxAngle, minAngle, a);
            return Mathf.SmoothStep(max, min, proportion);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float TransformDistance(this Transform a, Transform b)
        {
            if (a == null || b == null)
            {
                return 0f;
            }
            return Vector3.Distance(a.position, b.position);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool DistanceCheck(this Transform a, Transform b, float distance)
        {
            return Vector3.Distance(a.position, b.position) < distance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool DistanceCheck(this Vector3 a, Vector3 b, float distance)
        {
            return Vector3.Distance(a, b) < distance;
        }
        /// <summary>
        /// Checks if an object is within range
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="self"></param>
        /// <param name="user"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool WithinRange(this Transform self, bool enabled, Transform user, float range)
        {
            if (!enabled) return true;
            return Vector3.Distance(self.position, user.position) <= range;
        }
        /// <summary>
        /// Returns the largest value based on a Vector3
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static float LargestValue(this Vector3 vector3)
        {
            float x = vector3.x;
            float y = vector3.y;
            float z = vector3.z;
            
            float largestValue = x > y ? x : y;
            largestValue = z > largestValue ? z : largestValue;

            return largestValue;
        }
        [Serializable] public struct Cone
        {
            public Vector3 coneAxis, coneTipPosition;
            public float coneAngle, coneHeight;
            private float SinInternalAngle => Mathf.Sin(180f - (coneAngle + 90f));
            public float Radius => (Mathf.Sin(coneAngle) * coneHeight) / SinInternalAngle;

            public Cone (Vector3 tipPosition, Vector3 axis, float angle, float height = 1f)
            {
                coneAxis = axis;
                coneTipPosition = tipPosition;
                coneAngle = Mathf.Deg2Rad * angle;
                coneHeight = height;
            }
            public Cone (Transform origin, float angle, float height = 1f)
            {
                coneAxis = origin.forward;
                coneTipPosition = origin.position;
                coneAngle = Mathf.Deg2Rad * angle;
                coneHeight = height;
            }
        }
        /// <summary>
        /// Utility which checks whether a given point lies within a cone
        /// </summary>
        /// <returns></returns>
        public static bool IsPointInCone(this Vector3 point, Cone cone)
        {
            // Normalise the two vectors
            Vector3 axis = cone.coneAxis.normalized;
            Vector3 pointVector = (point - cone.coneTipPosition).normalized;
            
            // Use the dot product to calculate the angle between them
            float dotProduct = Vector3.Dot(pointVector, axis);
            float angle = Mathf.Acos(dotProduct);
            float coneRadians = Mathf.Lerp(0, cone.coneAngle, .5f) * Mathf.Deg2Rad;
            
            // Check against the height of the cone
            bool heightCheck = Vector3.Distance(point, cone.coneTipPosition) <= cone.coneHeight;
            
            // If the angle is smaller than the cone radians, then it is within the cone
            return angle < coneRadians && heightCheck;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool PointingAt(this Collider collider, Transform pointer, float angle, float range = 1f)
        {
            Cone cone = new Cone(origin: pointer, angle: angle, height: range);
            Vector3 point = collider.ClosestPoint(cone.coneTipPosition);
            return point.IsPointInCone(cone);
        }
        public enum Alignment
        {
            FacingTowards,
            FacingAway,
            Perpendicular
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool Aligned(Vector3 targetVector, Vector3 pointingVector, Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.FacingTowards:
                    return Vector3.Dot(targetVector, pointingVector) > 0;
                case Alignment.FacingAway:
                    return Vector3.Dot(targetVector, pointingVector) < 0;
                case Alignment.Perpendicular:
                    return Vector3.Dot(targetVector, pointingVector) == 0;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetVector"></param>
        /// <param name="pointingVector"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static bool PointingAt(this Vector3 pointingVector, Vector3 targetVector, float angle)
        { 
            return Aligned(targetVector, pointingVector, Alignment.FacingAway) && (180f - Vector3.Angle(targetVector, pointingVector)) <= angle;
        }
        /// <summary>
        /// r = √ ( x2 + y2 ), θ = tan-1 ( y / x )
        /// Converts (x, y) to (r, θ)
        /// </summary>
        /// <param name="cartesianCoordinate"></param>
        /// <returns></returns>
        public static CartesianPolarConverter.PolarCoordinates CartesianToPolar(Vector2 cartesianCoordinate)
        {
            return new CartesianPolarConverter.PolarCoordinates(
                rValue: (float) Math.Sqrt(Math.Pow(cartesianCoordinate.x, 2) + Math.Pow(cartesianCoordinate.y, 2)),
                θValue: (float) Math.Atan2(cartesianCoordinate.y, cartesianCoordinate.x));
        }
        /// <summary>
        /// x = r × cos( θ ), y = r × sin( θ )
        /// Converts (r, θ) to (x, y)
        /// </summary>
        /// <param name="polarCoordinate"></param>
        /// <returns></returns>
        public static Vector2 PolarToCartesian(CartesianPolarConverter.PolarCoordinates polarCoordinate)
        {
            float angleRad = (float) (Math.PI / 180.0) * ((polarCoordinate.θ * Mathf.Deg2Rad) - 90);
            return new Vector2((float) (polarCoordinate.r * Math.Cos(angleRad)), (float) (polarCoordinate.r * Math.Sin(angleRad)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static float NormalisedCartesianCoordinate(float value, float range)
        {
            return Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(-range, range, value));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="θ"></param>
        /// <returns></returns>
        public static float NormaliseRadianPolarCoordinate(float θ)
        {
            return Mathf.Lerp(0f, 1f, Mathf.InverseLerp(- (float) Math.PI, (float) Math.PI, θ));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="θ"></param>
        /// <returns></returns>
        public static float RadianNormalisedPolarCoordinate(float θ)
        {
            return Mathf.Lerp(-(float) Math.PI, (float) Math.PI, Mathf.InverseLerp(0f, 1f, θ));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prime"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector3 MirroredPosition(this Vector3 prime, Vector3 normal)
        {
            return Vector3.Reflect(inDirection: prime, inNormal: normal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prime"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Quaternion MirroredRotation(this Quaternion prime, Vector3 normal)
        {
            Quaternion mirrorRotation = new Quaternion(normal.x, normal.y, normal.z, 0); 
            Quaternion mirror = mirrorRotation * prime * mirrorRotation;
            return mirror;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mirror"></param>
        /// <param name="prime"></param>
        /// <param name="normal"></param>
        public static void MirrorLocal(this Transform mirror, Transform prime, Vector3 normal)
        {
            mirror.localPosition = prime.localPosition.MirroredPosition(normal);
            mirror.localRotation = prime.localRotation.MirroredRotation(normal);
            mirror.localScale = prime.localScale;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mirror"></param>
        /// <param name="prime"></param>
        /// <param name="normal"></param>
        public static void Mirror(this Transform mirror, Transform prime, Vector3 normal)
        {
            mirror.position = prime.position.MirroredPosition(normal);
            mirror.rotation = prime.rotation.MirroredRotation(normal);
            mirror.localScale = prime.localScale;
        }
    }
}
