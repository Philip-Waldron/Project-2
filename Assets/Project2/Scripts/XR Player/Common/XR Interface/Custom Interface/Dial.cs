using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VR_Prototyping.Plugins.Demigiant.DOTween.Modules;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.State_Transition;
using XR_Prototyping.Scripts.Utilities;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Custom_Interface
{
    [RequireComponent(typeof(CartesianPolarConverter))]
    public class Dial : XRInterfaceAbstraction
    {
        private enum DialType
        {
            Normal,
            Gestural
        }
        [Serializable] public class DialEvent : UnityEvent<float> {}
        
        [Header("Dial References")]
        [SerializeField] private Transform handle;
        [SerializeField] private TextMeshProUGUI dialLabel;
        [Header("Dial Settings")]
        [SerializeField] private CartesianPolarConverter.InteractionMethod interactionMethod = CartesianPolarConverter.InteractionMethod.Indirect;
        [SerializeField] private DialType dialType = DialType.Normal;
        [Header("Dial Data")] 
        [Range(0f, 1f)] public float value;
        public DialEvent onValueChanged;

        private CartesianPolarConverter CartesianPolarConverter => GetComponent<CartesianPolarConverter>();
        private float DialValue => CartesianPolarConverter.PolarCoordinate.θ;

        protected override void XRInterfaceAwake()
        {
            CartesianPolarConverter.SetClampedState(true);
            CartesianPolarConverter.SetPolarCoordinates(new CartesianPolarConverter.PolarCoordinates(rValue: 1, θValue: value));
        }

        protected override void XRInterfaceStart()
        {

        }

        protected override void XRInterfaceUpdate()
        {

        }

        public override void EngageStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(engaged, InterfaceState.Engaged, immediate);
        }

        public override void EngageStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            if (dialType == DialType.Gestural)
            {
                SetDialValue(check, direct);
            }
        }

        public override void EngageEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(normal, InterfaceState.Disengaged, immediate);
        }

        public override void SelectStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackStart);
            Transition(active, InterfaceState.Selected, immediate);
        }

        public override void SelectStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            SetDialValue(check, direct);
        }

        public override void SelectEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackEnd);
            Transition(direct ? normal : engaged, direct ? InterfaceState.Disengaged : InterfaceState.Engaged, immediate);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(bool state)
        {
            Enabled = state;
            label.SetText(state ? labelText : "");
            dialLabel.SetText(state ? labelText : "");
            foreach (Image image in interfaceImages)
            {
                image.enabled = state;
            }
            Collider.enabled = state;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="direct"></param>
        private void SetDialValue(XRInputController.Check check, bool direct)
        {
            CartesianPolarConverter.SetProxyPosition(direct
                ? XRInputController.Position(check)
                : interactionMethod == CartesianPolarConverter.InteractionMethod.Indirect
                    // Use the indirect raycast information
                    ? XRInteractionController.GetInteractionInformation(check).hit.point
                    // Use the closest point on the boundary
                    : Collider.ClosestPointOnBounds(XRInputController.Position(check)));
            
            // Set the visual state of the dial
            dialLabel.SetText($"{DialValue}");
            handle.position = CartesianPolarConverter.CoordinatePosition();
            
            // Call relevant methods
            onValueChanged.Invoke(DialValue);
            value = DialValue;
        }
    }
}