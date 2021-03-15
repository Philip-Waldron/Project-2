using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace XR_Prototyping.Scripts.Common.Locomotion
{
    [RequireComponent(typeof(Collider))]
    public class LocomotionTarget : MonoBehaviour
    {
        public enum TargetType
        {
            Volume,
            Point
        }
        [Header("Locomotion Target Behaviour")]
        public TargetType targetType = TargetType.Volume;
        public UnityEvent locomotionTrigger;
        [Space(10)]
        [Header("Locomotion Target Settings")]
        [SerializeField, Range(0, 5)] private float volumeOffset = 1f;
        [SerializeField] private Transform pointTarget;
        
        private Collider Collider => GetComponent<Collider>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float Offset()
        {
            return volumeOffset;
        }
        /// <summary>
        /// 
        /// </summary>
        public void PointTarget(out Vector3 position, out Vector3 forward)
        {
            position = pointTarget.position;
            forward = -pointTarget.forward;
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            OnDrawGizmos();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Transform volume = transform;
            Vector3 position = volume.position;
            Vector3 normalPosition = new Vector3(position.x, 0, position.z);
            switch (targetType)
            {
                case TargetType.Volume:
                    Gizmos.DrawRay(normalPosition, -volume.forward * Offset());
                    Gizmos.DrawRay(normalPosition, -volume.right * Collider.bounds.extents.x);
                    Gizmos.DrawRay(normalPosition, volume.right * Collider.bounds.extents.x);
                    break;
                case TargetType.Point:
                    Vector3 pointPosition = pointTarget.position;
                    Gizmos.DrawWireSphere(pointPosition, .05f);
                    Gizmos.DrawRay(pointPosition, pointTarget.forward * Offset());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
