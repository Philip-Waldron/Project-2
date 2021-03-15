using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.Feedback;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.UI;
using VR_Prototyping.Plugins.Demigiant.DOTween.Modules;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.State_Transition;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Unity_Interface_Abstractions
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class Button : XRInterfaceAbstraction
    {
        private UnityEngine.UI.Button UnityButton => GetComponent<UnityEngine.UI.Button>();

        protected override void XRInterfaceAwake()
        {
            Transition(normal, InterfaceState.Disengaged, immediate: true);
        }

        protected override void XRInterfaceStart()
        {

        }

        protected override void XRInterfaceUpdate()
        {

        }

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
        private void Disengage(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(normal, InterfaceState.Disengaged, immediate);
            engagementState = EngagementState.None;
            XRFeedbackController.Feedback(check, engaged.stateTransitionFeedbackEvent.feedbackEnd);
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
            UnityButton.onClick.Invoke();
            Transition(direct ? normal : engaged, direct ? InterfaceState.Disengaged : InterfaceState.Engaged, immediate);
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackEnd);
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
