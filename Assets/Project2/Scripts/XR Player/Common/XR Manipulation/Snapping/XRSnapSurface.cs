using System;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace XR_Prototyping.Scripts.Common.XR_Manipulation
{
    [RequireComponent(typeof(Collider))]
    public class XRSnapSurface : XRInputAbstraction
    {
        private Collider SnapCollider => GetComponent<Collider>();
        [SerializeField, Range(0f, 1f)] private float snapDistance = .1f, snapDuration = 1f;

        private void Awake()
        {
            SnapCollider.isTrigger = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private Vector3 SnapLocation(Vector3 currentPosition)
        {
            return SnapCollider.ClosestPoint(currentPosition);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ShouldSnap(Vector3 currentPosition, out Vector3 snapLocation, out float duration, out float threshold)
        {
            snapLocation = SnapLocation(currentPosition);
            float distance = Vector3.Distance(snapLocation, currentPosition);
            threshold = snapDistance;
            bool shouldSnap = distance <= snapDistance;
            duration = distance * snapDuration;
            return shouldSnap;
        }
    }
}