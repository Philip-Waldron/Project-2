using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;

namespace XR_Prototyping.Scripts.Common.Feedback
{
    public class XRFeedbackController : XRInputAbstraction
    {
        [Serializable] public struct FeedbackEvent
        {
            public FeedbackEventData feedbackStart, feedbackEnd;
        }
        /// <summary>
        /// 
        /// </summary>
        public enum CommonHaptics
        {
            NoCommonHaptics,
            AllCommonHaptics,
            GrabOnly,
            SelectOnly,
        }
        /// <summary>
        /// 
        /// </summary>
        public enum Ratcheting
        {
            NoRatcheting,
            PervasiveRatcheting,
            OnlyInterfaceRatcheting
        }
        /// <summary>
        /// 
        /// </summary>
        public enum TouchClick
        {
            Disabled,
            Enabled
        }
        [Header("Common Feedback Events")] 
        [SerializeField] private CommonHaptics commonHaptics = CommonHaptics.GrabOnly;
        [SerializeField] public FeedbackEvent select, grab;
        [Header("Touchstrip Ratcheting")] 
        [SerializeField] private Ratcheting ratcheting = Ratcheting.PervasiveRatcheting;
        [SerializeField] private TouchstripRatchetFeedback touchstripRatcheting;
        [SerializeField, Range(float.Epsilon, .01f)] private float threshold = float.Epsilon;
        [Header("Touchstrip Clicking")] 
        [SerializeField] private TouchClick touchClick;
        [SerializeField] private FeedbackEvent touchClickFeedback;

        [Serializable] private class TouchstripRatchetFeedback
        {
            public FeedbackEventData ratchet;
            [SerializeField, Range(0, 11)] private int divisions = 5;
            private static float TouchstripValue => XRInputController.AxisValue(check: XRInputController.DominantHand(), truncated: true).y;
            public static double RoundedTouchstripValue => Math.Round(TouchstripValue, 3);
            public float Increment => 2f / divisions;
            public const int Minimum = -1, Maximum = 1;
        }

        private void Update()
        {
            if (touchClick == TouchClick.Enabled)
            {
                TouchClickHaptics();
            }

            switch (ratcheting)
            {
                case Ratcheting.NoRatcheting:
                    return;
                case Ratcheting.PervasiveRatcheting:
                    TouchstripRatchet(enableRatcheting: true);
                    break;
                case Ratcheting.OnlyInterfaceRatcheting:
                    TouchstripRatchet(enableRatcheting: XRInteractionController.GetEngagedState(check: XRInputController.DominantHand()));
                    break;
                default:
                    return;
            }
            
            switch (commonHaptics)
            {
                case CommonHaptics.NoCommonHaptics:
                    break;
                case CommonHaptics.GrabOnly:
                    GrabHaptics();
                    break;
                case CommonHaptics.SelectOnly:
                    SelectHaptics();
                    break;
                case CommonHaptics.AllCommonHaptics:
                    SelectHaptics();
                    GrabHaptics();
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SelectHaptics()
        {
            if (XRInputController.InputEvent(XRInputController.Event.TriggerPress).State(XRInputController.InputEvents.InputEvent.Transition.Down, out XRInputController.Check down))
            {
                Feedback(down, select.feedbackStart);
            }
            else if (XRInputController.InputEvent(XRInputController.Event.TriggerPress).State(XRInputController.InputEvents.InputEvent.Transition.Up, out XRInputController.Check up))
            {
                Feedback(up, select.feedbackEnd);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void GrabHaptics()
        {
            if (XRInputController.InputEvent(XRInputController.Event.GripPress).State(XRInputController.InputEvents.InputEvent.Transition.Down, out XRInputController.Check down))
            {
                Feedback(down, grab.feedbackStart);
            }
            else if (XRInputController.InputEvent(XRInputController.Event.GripPress).State(XRInputController.InputEvents.InputEvent.Transition.Up, out XRInputController.Check up))
            {
                Feedback(up, grab.feedbackEnd);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void TouchstripRatchet(bool enableRatcheting)
        {
            if (!enableRatcheting || !XRInputController.TouchpadSegmentation().TouchstripTouch()) return;
            for (float i = TouchstripRatchetFeedback.Minimum; i <= TouchstripRatchetFeedback.Maximum; i += touchstripRatcheting.Increment)
            {
                if (Math.Abs(Math.Abs(TouchstripRatchetFeedback.RoundedTouchstripValue) - Math.Abs(Math.Round(i, 3))) < threshold)
                {
                    Feedback(check: XRInputController.DominantHand(), touchstripRatcheting.ratchet);
                    return;
                } 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void TouchClickHaptics()
        {
            if (XRInputController.InputEvent(XRInputController.Event.AnalogClick).State(check: XRInputController.DominantHand(), XRInputController.InputEvents.InputEvent.Transition.Down))
            {
                Feedback(check: XRInputController.DominantHand(), touchClickFeedback.feedbackStart);
            }
            else if (XRInputController.InputEvent(XRInputController.Event.AnalogClick).State(check: XRInputController.DominantHand(), XRInputController.InputEvents.InputEvent.Transition.Up))
            {
                Feedback(check: XRInputController.DominantHand(), touchClickFeedback.feedbackEnd);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="data"></param>
        public static void Feedback(XRInputController.Check check, FeedbackEventData data)
        {
            if (data == null) return;
            XRInputController.InputDevice(check).HapticEvent(data);
            AudioSource(check).AudioFeedback(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="data"></param>
        public static void ExternalFeedback(XRInputController.Check check, FeedbackEventData data)
        {
            XRInputController.InputDevice(check).HapticEvent(data);
            AudioSource(check).AudioFeedback(data);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void StopHapticEvent(XRInputController.Check check)
        {
            XRInputController.InputDevice(check).StopHaptics();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private static AudioSource AudioSource(XRInputController.Check check)
        {
            return XRInputController.Transform(check).TryGetComponent(out AudioSource audioSource) ? audioSource : null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="common"></param>
        /// <param name="ratchet"></param>
        public void SetHapticType(CommonHaptics common, Ratcheting ratchet)
        {
            commonHaptics = common;
            ratcheting = ratchet;
        }
    }
}
