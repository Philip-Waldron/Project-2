using System;
using System.Collections.Generic;
using UnityEngine;

namespace XR_Prototyping.Scripts.Utilities.Generic
{
    public static class Set
    {
        private const float Damping = 1f;
        public enum Axis
        {
            Right,
            Up,
            Forward
        }
        /// <summary>
        /// Sets transform A's position to transform B's position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Position(this Transform a, Transform b)
        {
            a.transform.position = b.transform.position;
        }
        /// <summary>
        /// Sets transform A's rotation to transform B's rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Rotation(this Transform a, Transform b)
        { 
            a.transform.rotation = b.transform.rotation;
        }
        /// <summary>
        /// Sets transform A's local position and rotation to zero
        /// </summary>
        /// <param name="a"></param>
        public static void ResetLocalTransform(this Transform a)
        {
            Transform transform = a.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        /// <summary>
        /// Sets transform A's local position and rotation to transform B's local position and rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void LocalTransforms(this Transform a, Transform b)
        {
            Transform transform = a.transform;
            transform.localPosition = b.localPosition;
            transform.localRotation = b.localRotation;
        }
        /// <summary>
        /// Transform A looks at transform B, but maintains it's vertical axis
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void LookAtVertical(this Transform a, Transform b)
        {
            a.LookAwayFrom(b, Vector3.up);
            a.eulerAngles = new Vector3(0, a.eulerAngles.y,0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="heightTransform"></param>
        /// <param name="normalise"></param>
        /// <param name="local"></param>
        public static void SplitPosition(this Transform target, Transform heightTransform, bool normalise = false, bool local = false)
        {
            Vector3 position = target.position;
            if (local)
            {
                target.transform.localPosition = new Vector3(normalise ? 0f : position.x, heightTransform.localPosition.y, normalise ? 0f : position.z);
            }
            else
            {
                target.transform.position = new Vector3(normalise ? 0f : position.x, heightTransform.position.y, normalise ? 0f : position.z);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="split"></param>
        /// <param name="normalise"></param>
        /// <param name="local"></param>
        public static void SplitPosition(this Transform target, Vector3 split, bool normalise = false, bool local = false)
        {
            if (local)
            {
                target.transform.localPosition = new Vector3(split.x, normalise ? 0f: target.localPosition.y, split.z);
            }
            else
            {
                target.transform.position = new Vector3(split.x, normalise ? 0f: target.position.y, split.z);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void SplitTransform(this Transform target, Transform positionTarget, Transform heightReference)
        {
            if (positionTarget == null || heightReference == null || target == null) return;
            Vector3 position = positionTarget.position;
            target.transform.position = new Vector3(position.x, heightReference.position.y, position.z);
        }
        /// <summary>
        /// Transform C will follow transform XZ's x and z position, and transform Y's y position
        /// </summary>
        /// <param name="c"></param>
        /// <param name="y"></param>
        /// <param name="xz"></param>
        public static void SplitPositionVector(this Transform c, float y, Transform xz)
        {
            if (xz == null || c == null) return;
            Vector3 position = xz.position;
            c.transform.position = new Vector3(position.x, y, position.z);
        }
        /// <summary>
        /// Transform C will follow transform XZ's x and z position, and transform Y's y position
        /// </summary>
        /// <param name="c"></param>
        /// <param name="xz"></param>
        /// <param name="y"></param>
        public static void PositionSplit(this Transform c, Transform xz, Transform y)
        {
            if (xz == null || y == null || c == null) return;
            Vector3 position = xz.position;
            c.transform.position = new Vector3(position.x, y.position.y, position.z);
        }
        /// <summary>
        /// Controller will follow the y-rotation of target, follow determines of it follows the position
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="target"></param>
        /// <param name="follow"></param>
        public static void SplitRotation(this Transform controller, Transform target, bool follow)
        {
            if (controller == null || target == null) return;
            Vector3 c = controller.eulerAngles;
            target.transform.eulerAngles = new Vector3(0, c.y, 0);
            
            if(!follow) return;
            Position(target, controller);
        }
        /// <summary>
        /// Transform A will lerp to transform B's position
        /// </summary>
        /// <param name="target"></param>
        /// <param name="destination"></param>
        /// <param name="value"></param>
        public static void LerpPosition(this Transform target, Transform destination, float value)
        {
            target.position = Vector3.Lerp(target.position, destination.position, value);
        }

        /// <summary>
        /// Transform A will lerp to transform B's position
        /// </summary>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="from"></param>
        public static void LerpMidpoint(this Transform target, Transform from, Transform to, float value)
        {
            Vector3 midpoint = Vector3.Lerp(from.position, to.position, .5f);
            target.position = Vector3.Lerp(target.position, midpoint, value);
        }
        /// <summary>
        /// Transform A will lerp to transform B's rotation
        /// </summary>
        /// <param name="target"></param>
        /// <param name="destination"></param>
        /// <param name="value"></param>
        public static void LerpRotation(this Transform target, Transform destination, float value)
        {
            target.rotation = Quaternion.Lerp(target.rotation, destination.rotation, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="destination"></param>
        /// <param name="value"></param>
        public static void LerpScale(this Transform target, Transform destination, float value)
        {
            target.localScale = Vector3.Lerp(target.localScale, destination.localScale, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="destination"></param>
        /// <param name="value"></param>
        public static void LerpScale(this Transform target, Vector3 destination, float value)
        {
            target.localScale = Vector3.Lerp(target.localScale, destination, value);
        }
        /// <summary>
        /// Transform A will lerp to transform B's position
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="smoothing"></param>
        public static void LerpPosition(this Transform target, Vector3 position, float smoothing)
        {
            target.position = Vector3.Lerp(target.position, position, smoothing);
        }
        /// <summary>
        /// Transform A will lerp to transform B's local position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void VectorLerpLocalPosition(this Transform a, Vector3 b, float l)
        {
            if (a == null) return;
            a.localPosition = Vector3.Lerp(a.localPosition, b, l);
        }
        /// <summary>
        /// Sets transform A's position and rotation to transform B's position and rotation
        /// </summary>
        /// <param name="target"></param>
        /// <param name="follow"></param>
        /// <param name="scale"></param>
        public static void Transforms(this Transform target, Transform follow, bool scale = false)
        {
            target.LerpTransforms(follow, Damping, scale);
        }
        /// <summary>
        /// Sets transform A's position to transform B's position, but will lerp their rotations
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void StableTransforms(this Transform a, Transform b, float l)
        {
            Position(a, b);
            LerpRotation(a, b, l);
        }
        /// <summary>
        /// Sets transform A's position to transform B's position, but wil lerp their rotations
        /// </summary>
        /// <param name="a"></param>
        /// <param name="position"></param>
        /// <param name="look"></param>
        /// <param name="away"></param>
        public static void StableTransformLook(this Transform a, Transform position, Transform look, bool away)
        {
            Position(a, position);
            if (!away)
            {
                a.LookAt(look, a.up);
            }
            else
            {
                a.LookAwayFrom(look, a.up);
            }
        }
        /// <summary>
        /// Sets transform A's position to transform B's position, but wil lerp their rotations
        /// </summary>
        /// <param name="a"></param>
        /// <param name="position"></param>
        /// <param name="look"></param>
        /// <param name="away"></param>
        public static void StablePositionLook(this Transform a, Vector3 position, Transform look, bool away)
        {
            a.transform.position = position;
            if (!away)
            {
                a.LookAt(look, a.up);
            }
            else
            {
                a.LookAwayFrom(look, a.up);
            }
        }
        /// <summary>
        /// Sets a transform to (0,0,0) and (0,0,0,0)
        /// </summary>
        /// <param name="transform"></param>
        public static void DefaultTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Linearly interpolates transform A's position and rotation to transform B's position and rotation
        /// </summary>
        /// <param name="target"></param>
        /// <param name="destination"></param>
        /// <param name="damping"></param>
        /// <param name="scale"></param>
        public static void LerpTransforms(this Transform target, Transform destination, float damping, bool scale = false)
        {
            LerpPosition(target, destination, damping);
            LerpRotation(target, destination, damping);
            if (scale)
            {
                LerpScale(target, destination, damping);
            }
        }
        /// <summary>
        /// Returns distance to the midpoint of transform A and B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MidpointDistance(this Transform a, Transform b)
        {
            return Vector3.Distance(a.position, b.position) *.5f;
        }
        /// <summary>
        /// Returns distance to the midpoint of Vector3s A and B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MidpointDistance(this Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) *.5f;
        }
        public static void ForwardVector(this Transform a, Transform b)
        {
            if (a == null || b == null) return;

            a.forward = b.forward;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 MidpointPosition(Transform a, Transform b)
        {
            Vector3 posA = a.position;
            Vector3 posB = b.position;
            return Vector3.Lerp(posA, posB, .5f);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 MidpointPosition(Vector3 a, Vector3 b)
        {
            Vector3 posA = a;
            Vector3 posB = b;
            return Vector3.Lerp(posA, posB, .5f);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static void Width(this LineRenderer lr, float start, float end)
        {
            lr.startWidth = start;
            lr.endWidth = end;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="width"></param>
        public static void Width(this LineRenderer lr, float width)
        {
            lr.startWidth = width;
            lr.endWidth = width;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalScale"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Vector3 LocalScale(this Vector3 originalScale, float factor)
        {
            return new Vector3(
                originalScale.x + originalScale.x * factor,
                originalScale.y + originalScale.y * factor,
                originalScale.z + originalScale.z * factor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Vector3 LocalPosition(this Vector3 originalPos, float factor)
        {
            return new Vector3(
                originalPos.x,
                originalPos.y,
                originalPos.z + factor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="z"></param>
        /// <param name="lerp"></param>
        /// <param name="speed"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void LocalDepth(this Transform a, float z, bool lerp, float speed)
        {
            if (a == null) return;
            Vector3 p = a.localPosition;

            switch (lerp)
            {
                case false:
                    a.localPosition = new Vector3(p.x,p.y, z);
                    break;
                case true:
                    Vector3.Lerp(a.localPosition, new Vector3(p.x, p.y, z), speed);
                    break;
                default:
                    throw new ArgumentException();
            }   
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector3 Offset(this Transform a, Transform b)
        {
            Vector3 x = a.position;
            Vector3 y = b.position;
            Vector3 xN = new Vector3(x.x, 0, x.z);
            Vector3 yN = new Vector3(y.x, 0, y.z);
            
            return yN - xN;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static Vector3 Offset(float depth)
        {
            return new Vector3(x: 0, y: 0, z: depth);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Divergence(this Transform a, Transform b)
        {           
            return Vector3.Angle(a.forward, b.forward);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transform"></param>
        /// <param name="upwards"></param>
        public static void LookAwayFrom(this Transform target, Transform transform, Vector3 upwards) 
        {
            target.rotation = Quaternion.LookRotation(target.position - transform.position, upwards);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="upwards"></param>
        public static void LookAwayFrom(this Transform target, Vector3 position, Vector3 upwards) 
        {
            target.rotation = Quaternion.LookRotation(target.position - position, upwards);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public static void ReverseNormals(this MeshFilter filter)
        {
            Mesh mesh = filter.mesh;
            Vector3[] normals = mesh.normals;
            
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m=0;m<mesh.subMeshCount;m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                
                for (int i=0;i<triangles.Length;i+=3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                
                mesh.SetTriangles(triangles, m);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="axis"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void LockLocalPositionAboutAxis(this Transform transform, Axis axis)
        {
            Vector3 targetLocalPosition = transform.localPosition;
            switch (axis)
            {
                case Axis.Right:
                    transform.localPosition = new Vector3(targetLocalPosition.x, 0, 0);
                    break;
                case Axis.Up:
                    transform.localPosition = new Vector3(0, targetLocalPosition.y, 0);
                    break;
                case Axis.Forward:
                    transform.localPosition = new Vector3(0, 0, targetLocalPosition.z);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="axis"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void LockRotationAboutAxis(this Transform transform, Axis axis)
        {
            Vector3 localRotation = transform.localEulerAngles;
            switch (axis)
            {
                case Axis.Right:
                    transform.localEulerAngles = new Vector3(localRotation.x, 0, 0);
                    break;
                case Axis.Up:
                    transform.localEulerAngles = new Vector3(0, localRotation.y, 0);
                    break;
                case Axis.Forward:
                    transform.localEulerAngles = new Vector3(0, 0, localRotation.z);
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// Returns a GameObject
        /// Sets its parent, name, position, and tag
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="tag"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public static GameObject Object(GameObject parent, string name, Vector3 position, bool local = true, string tag = null)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.parent = parent == null ? null : parent.transform;
            gameObject.name = name;
            if (local)
            {
                gameObject.transform.localPosition = position;
            }
            else
            {
                gameObject.transform.position = position;
            }
            if (tag != null)
            {
                gameObject.tag = tag;
            }
            return gameObject;
        }
        ///<summary>
        /// Creates a bounds using the MeshRenderers of the provided transform
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Bounds BoundsOfChildren(this Transform parentObject, Bounds bounds)
        {
            bounds = new Bounds (parentObject.position, Vector3.zero);
            MeshRenderer[] renderers = parentObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }
        /// <summary>
        /// Rounds Vector3 to the nearest decimal place
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="fraction"></param>
        /// <param name="roundDown"></param>
        /// <returns></returns>
        public static Vector3 Round(this Vector3 vector3, float fraction = .25f, bool roundDown = false)
        {
            return new Vector3(
                vector3.x.Round(fraction, roundDown),
                vector3.y.Round(fraction, roundDown),
                vector3.z.Round(fraction, roundDown)
                );
        }
        /// <summary>
        /// Rounds a provided float to the nearest supplied "round"
        /// ie. 105.5 up to nearest 0.3 = 105.6
        /// </summary>
        /// <param name="preRound"></param>
        /// <param name="round"></param>
        /// <param name="roundDown"></param>
        /// <returns></returns>
        private static float Round(this float preRound, float round, bool roundDown = false)
        {
            return Math.Abs(round) < float.Epsilon ?
                preRound : 
                roundDown ? 
                (float) Math.Floor(preRound / round) * round : 
                (float) Math.Ceiling(preRound / round) * round;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Vector3 ScaleFactor(float factor)
        {
            return new Vector3(factor, factor, factor);
        }
        /// <summary>
        /// Sets the local scale of the target object to new Vector3(factor, factor, factor)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="factor"></param>
        /// <param name="damping"></param>
        /// <returns></returns>
        public static void ScaleFactor(this Transform target, float factor, float damping = 1f)
        {
            target.localScale = Vector3.Lerp(target.localScale, ScaleFactor(factor), damping);
        }
        /// <summary>
        /// 
        /// </summary>
        public static Vector3 ClampScale(this Vector3 current, float minimum, float maximum)
        {
            float value = Mathf.Clamp(current.x, minimum, maximum);
            return ScaleFactor(value);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void ClampScale(this Transform target, float minimum, float maximum)
        {
            target.localScale = ClampScale(target.localScale, minimum, maximum);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Vector3 NormalisedPosition(Vector3 position, float height = 0f)
        {
            return new Vector3(position.x, height, position.z);
        }
        /// <summary>
        /// Sets the width and height of a rect transform
        /// </summary>
        public static void RectTransformSize(this RectTransform rectTransform, float size)
        {
            rectTransform.sizeDelta = new Vector2(
                size, 
                size);
        }
        /// <summary>
        /// Remaps some input float to fit within a new range
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromMinimum"></param>
        /// <param name="fromMaximum"></param>
        /// <param name="toMinimum"></param>
        /// <param name="toMaximum"></param>
        /// <returns></returns>
        public static float Remap(this float value, float fromMinimum, float fromMaximum, float toMinimum, float toMaximum) 
        {
            float fromAbsolute  =  value - fromMinimum;
            float fromMaxAbsolute = fromMaximum - fromMinimum;
            float normal = fromAbsolute / fromMaxAbsolute;
            float toMaxAbsolute = toMaximum - toMinimum;
            float toAbsolute = toMaxAbsolute * normal;
            float remapped = toAbsolute + toMinimum;
            return remapped;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void HitTransform(this Transform target, RaycastHit hit, bool normal = true)
        {
            target.position = hit.point;
            target.forward = normal ? hit.normal : target.forward;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="objects"></param>
        /// <param name="axis"></param>
        /// <param name="radius"></param>
        /// <param name="orientate"></param>
        public static void CircularPositioning(this Transform center, List<Transform> objects, Axis axis, float radius, bool orientate = true)
        {
            for (int i = 0; i < objects.Count; ++i)
            {
                float theta = (2 * (float) Math.PI / objects.Count) * i;
                Vector3 local;
                
                switch (axis)
                {
                    case Axis.Right:
                        local = new Vector3(0f, radius * (float) Math.Cos(theta), radius * (float) Math.Sin(theta));
                        break;
                    case Axis.Up:
                        local = new Vector3(radius * (float) Math.Cos(theta), 0f, radius * (float) Math.Sin(theta));
                        break;
                    case Axis.Forward:
                        local = new Vector3(radius * (float) Math.Cos(theta), radius * (float) Math.Sin(theta), 0f);
                        break;
                    default:
                        return;
                }
                
                objects[i].localPosition = local;
                
                if (!orientate) continue;
                objects[i].LookAwayFrom(center, Vector3.up);
                Vector3 rotation = objects[i].localEulerAngles;
                objects[i].localEulerAngles = new Vector3(rotation.x, rotation.y, 0f);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void LookAt(this List<Transform> objects, Transform center, bool lookAway = false)
        {
            foreach (Transform target in objects)
            {
                if (lookAway)
                {
                    target.LookAwayFrom(center, Vector3.up);
                }
                else
                {
                    target.LookAt(center);
                }
            }
        }
    }
}
