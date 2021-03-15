using UnityEngine;

namespace XR_Prototyping.Scripts.Utilities.Extensions
{
    public static class RigidbodyExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="mass"></param>
        /// <param name="drag"></param>
        /// <param name="useGravity"></param>
        /// <returns></returns>
        public static Rigidbody AddConfiguredRigidbody(this GameObject gameObject, float mass = 1f, float drag = 1f, bool useGravity = false)
        {
            Rigidbody rigidbody = gameObject.TryGetComponent(out Rigidbody attachedRigidbody) ? attachedRigidbody : gameObject.AddComponent<Rigidbody>();
            rigidbody.SetRigidbodyData(mass, drag, useGravity);
            return rigidbody;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="mass"></param>
        /// <param name="drag"></param>
        /// <param name="useGravity"></param>
        public static void SetRigidbodyData(this Rigidbody rigidbody, float mass = 1f, float drag = 1f, bool useGravity = false)
        {
            rigidbody.mass = mass;
            rigidbody.drag = drag;
            rigidbody.angularDrag = drag;
            rigidbody.useGravity = useGravity;
        }
    }
}