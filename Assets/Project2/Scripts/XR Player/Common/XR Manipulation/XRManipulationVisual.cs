using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Manipulation
{
    public class XRManipulationVisual : XRInputAbstraction
    {
        [SerializeField] private Transform visual;
        [SerializeField] private XRInputController.Hand hand = XRInputController.Hand.Dominant;
        private XRInputController.Check Check => XRInputController.CheckHand(hand);
        private static float ScaleFactor => XRManipulationController.GetInteractionRange() * 2f;
        
        private void Update()
        {
            visual.Transforms(XRInputController.Transform(Check));
            visual.ScaleFactor(ScaleFactor);
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnValidate()
        {
            visual.ScaleFactor(ScaleFactor);
        }
    }
}