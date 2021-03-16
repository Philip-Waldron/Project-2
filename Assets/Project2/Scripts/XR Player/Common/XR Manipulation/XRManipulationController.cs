using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Movement;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Manipulation
{
    [RequireComponent(typeof(XRMovementController))]
    public class XRManipulationController : XRInputAbstraction
    {
        private XRMovementController XRMovementController => GetComponent<XRMovementController>();

        [SerializeField, Range(1f, 5f)] public float lassoOffset;

        public XRManipulationInformation left, right;
        
        private void Start()
        {
            left = gameObject.AddComponent<XRManipulationInformation>();
            right = gameObject.AddComponent<XRManipulationInformation>();
            
            left.SetupManipulation(this, XRInputController.Check.Left);
            right.SetupManipulation(this, XRInputController.Check.Right);
        }

        private void Update()
        {
            left.ManipulationLogic();
            right.ManipulationLogic();
        }
    }
}