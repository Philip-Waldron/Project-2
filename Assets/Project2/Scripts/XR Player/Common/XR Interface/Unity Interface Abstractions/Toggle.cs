using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.UI;
using VR_Prototyping.Plugins.Demigiant.DOTween.Modules;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.State_Transition;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Unity_Interface_Abstractions
{
    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public class Toggle : XRInterfaceAbstraction
    {
        [Header("Toggle Settings")]
        [SerializeField] private Image toggleIcon;
        [SerializeField] private Color onColour = Color.white, offColour = Color.black;
        [SerializeField] private XRStateTransition toggledOn;
        private UnityEngine.UI.Toggle UnityToggle => GetComponent<UnityEngine.UI.Toggle>();

        private bool toggleState;
        protected override void XRInterfaceAwake()
        {
            ReadOnlySetToggleState(UnityToggle.isOn);
            UnityToggle.onValueChanged.AddListener(ReadOnlySetToggleState);
        }

        protected override void XRInterfaceStart()
        {

        }

        protected override void XRInterfaceUpdate()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="immediate"></param>
        /// <param name="direct"></param>
        public override void EngageStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            switch (engagementState)
            {
                case EngagementState.None:
                    Engage(check, immediate);
                    return;
                case EngagementState.Single:
                    engagementState = EngagementState.Double;
                    return;
                case EngagementState.Double:
                    return;
                default:
                    return;
            }
        }   
        private void Engage(XRInputController.Check check, bool immediate = false)
        {
            XRFeedbackController.Feedback(check, engaged.stateTransitionFeedbackEvent.feedbackStart);
            engagementState = EngagementState.Single;
            Transition(engaged, InterfaceState.Engaged, immediate);
        }
        public override void EngageStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {

        }
        public override void EngageEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            switch (engagementState)
            {
                case EngagementState.None:
                    return;
                case EngagementState.Single:
                    Disengage(check, immediate);
                    return;
                case EngagementState.Double:
                    engagementState = EngagementState.Single;
                    return;
                default:
                    return;
            }
        }
        private void Disengage(XRInputController.Check check, bool immediate = false)
        {
            XRFeedbackController.Feedback(check, engaged.stateTransitionFeedbackEvent.feedbackEnd);
            engagementState = EngagementState.None;
            Transition(UnityToggle.isOn ? toggledOn : normal, InterfaceState.Disengaged, immediate);
        }
        public override void SelectStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(active, InterfaceState.Selected, immediate);
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackStart);
        }

        public override void SelectStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {

        }

        public override void SelectEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            UnityToggle.isOn = !UnityToggle.isOn;
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackEnd);
            Transition(UnityToggle.isOn ? toggledOn : engaged, InterfaceState.Engaged, immediate);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetToggleState(bool state)
        {
            UnityToggle.isOn = state;
            ReadOnlySetToggleState(state);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void ReadOnlySetToggleState(bool state)
        {
            toggleIcon.color = state ? onColour : offColour;
            label.color = state ? onColour : offColour;
            Transition(state ? toggledOn : normal, InterfaceState.Disengaged, immediate: true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(bool state)
        {
            Collider.enabled = state;
            label.SetText(state ? labelText : "");
            foreach (Image image in interfaceImages)
            {
                image.enabled = state;
            }
        }
    }
}
