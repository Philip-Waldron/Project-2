using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Input.Input_Data;
using UnityEngine;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;

namespace Project2.Scripts.XR_Player.Common.Feedback
{
    public class XRFeedbackController : XRInputAbstraction
    {
        [Serializable] public struct FeedbackEvent
        {
            public FeedbackEventData feedbackStart, feedbackEnd;
        }
        public FeedbackEventData buttonDown, buttonUp;
        private void Update() => GrabHaptics();
        /// <summary>
        /// 
        /// </summary>
        private void GrabHaptics()
        {
            if (XRInputController.InputEvent(XRInputController.Event.GripPress).State(InputEvents.InputEvent.Transition.Down, out XRInputController.Check down))
            {
                Feedback(down, buttonDown);
            }
            else if (XRInputController.InputEvent(XRInputController.Event.GripPress).State(InputEvents.InputEvent.Transition.Up, out XRInputController.Check up))
            {
                Feedback(up, buttonUp);
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
    }
}
