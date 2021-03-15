using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.XR_Debug;

namespace XR_Prototyping.Scripts.Common
{
    public class XRInteractionOrigin : XRInputAbstraction
    {
        [SerializeField] private Transform origin;
        private XRControllerBridge XRControllerBridge => GetComponentInParent<XRControllerBridge>();
        private XRInputController.Check Check => XRControllerBridge.XRControllerCheck();

        private void Start()
        {
            origin = origin == null ? transform : origin;
            XRInteractionController.SetXRInteractionOrigin(this, Check);
        }
        private void Update()
        {
            if (!XRDebug.Enabled) return;
            XRDebug.DrawRay(index: $"{GetInstanceID()} Forward", Position(), Forward(), .25f, Color.blue);
            XRDebug.DrawRay(index: $"{GetInstanceID()} Right", Position(), Transform().right, .05f, Color.red);
            XRDebug.DrawRay(index: $"{GetInstanceID()} Up", Position(), Transform().up, .05f, Color.green);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Transform Transform()
        {
            return origin;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Rotation()
        {
            return Transform().eulerAngles;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Quaternion Quaternion()
        {
            return Transform().rotation;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Position()
        {
            return Transform().position;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Forward()
        {
            return Transform().forward;
        }
    }
}
