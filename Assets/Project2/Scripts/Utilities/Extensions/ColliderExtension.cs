using UnityEngine;

namespace XR_Prototyping.Scripts.Utilities.Extensions
{
    public static class ColliderExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="radius"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static SphereCollider AddConfiguredSphereCollider(this GameObject target, float radius, bool trigger)
        {
            SphereCollider collider = target.TryGetComponent(out SphereCollider attachedCollider) ? attachedCollider : target.AddComponent<SphereCollider>();
            collider.radius = radius;
            collider.isTrigger = trigger;
            return collider;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static CapsuleCollider AddConfiguredCapsuleCollider(this GameObject target, float radius, float height, bool trigger)
        {
            CapsuleCollider collider = target.TryGetComponent(out CapsuleCollider attachedCollider) ? attachedCollider : target.AddComponent<CapsuleCollider>();
            collider.radius = radius;
            collider.height = height;
            collider.isTrigger = trigger;
            return collider;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="convex"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static MeshCollider AddConfiguredMeshCollider(this GameObject target, bool convex, bool trigger)
        {
            MeshCollider collider = target.TryGetComponent(out MeshCollider attachedCollider) ? attachedCollider : target.AddComponent<MeshCollider>();
            collider.convex = convex;
            collider.isTrigger = trigger;
            return collider;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static BoxCollider AddConfiguredBoxCollider(this GameObject target, bool trigger)
        {
            BoxCollider collider = target.TryGetComponent(out BoxCollider attachedCollider) ? attachedCollider : target.AddComponent<BoxCollider>();
            collider.isTrigger = trigger;
            return collider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trigger"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static BoxCollider AddConfiguredBoxCollider(this GameObject target, bool trigger, Vector3 size)
        {
            BoxCollider collider = target.AddConfiguredBoxCollider(trigger);
            collider.size = size;
            return collider;
        }
    }
}
