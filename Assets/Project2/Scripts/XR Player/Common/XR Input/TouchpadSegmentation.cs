using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace XR_Prototyping.Scripts.Common.XR_Input
{
    public class TouchpadSegmentation : XRInputAbstraction
    {
        [Header("Touchstrip Segmentation Values")]
        [SerializeField, Range(MaximumValue, MinimumValue)] private float topBoundary = .33f;
        [SerializeField, Range(MaximumValue, MinimumValue)] private float bottomBoundary = -.33f;
        [Header("Touchstrip Readout Values")]
        [SerializeField, Range(MaximumValue, MinimumValue)] private float rawValue;
        [SerializeField, Range(MaximumValue, MinimumValue)] private float truncatedValue;
        public State state;
        [Header("Touch Click Information")]
        [SerializeField, Range(0f, 1f)] private float gripClickThreshold = .25f;
        [SerializeField, Range(0f, 1f)] private float touchClickThreshold = .75f;
        [SerializeField, Range(0f, 1f)] private float primaryValue;
        private Vector2 TopZone => new Vector2(MaximumValue, topBoundary);
        private Vector2 CenterZone => new Vector2(topBoundary, bottomBoundary);
        private Vector2 BottomZone => new Vector2(bottomBoundary, MinimumValue);
        private static float AbstractedPrimaryValue => XRInputController.GrabValue(check: XRInputController.DominantHand());
        
        public enum State
        {
            None,
            Select,
            Grab,
            TouchstripTouch,
            TouchstripClick
        }

        private static float TouchpadValue => XRInputController.AxisValue(check: XRInputController.DominantHand()).y;
        private static bool ValidValue => XRInputController.Touch(check: XRInputController.DominantHand(), XRInputController.TouchType.AnalogInput);

        private const float MaximumValue = 1f;
        private const float MinimumValue = -1f;

        private void Update()
        {
            primaryValue = AbstractedPrimaryValue;
            rawValue = TouchpadValue;
            truncatedValue = TouchstripValue().y;

            if (!ValidValue)
            {
                state = State.None;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Grab()
        {
            bool valid = WithinRange(TouchpadValue, TopZone) && ValidValue && AbstractedPrimaryValue >= gripClickThreshold; 
            if (valid)
            {
                state = State.Grab;
            }
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Select()
        {
            bool valid = WithinRange(TouchpadValue, CenterZone) && ValidValue;
            if (valid)
            {
                state = State.Select;
            }
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TouchstripTouch()
        {
            bool valid = WithinRange(TouchpadValue, BottomZone);
            if (valid)
            {
                state = State.TouchstripTouch;
            }
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TouchstripClick()
        {
            bool valid = TouchstripTouch() && AbstractedPrimaryValue >= touchClickThreshold;
            if (valid)
            {
                state = State.TouchstripClick;
            }
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2 TouchstripValue()
        {
            Vector2 remappedValues = new Vector2(XRInputController.AxisValue(check: XRInputController.DominantHand()).x,
                y: Mathf.Lerp(a: MaximumValue, b: MinimumValue,
                    t: Mathf.InverseLerp(a: bottomBoundary, b: MinimumValue, TouchpadValue)));
            return TouchstripTouch() ? remappedValues : Vector2.zero;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private static bool WithinRange(float value, Vector2 range)
        {
            return (value <= range.x && value >= range.y);
        }
        /// <summary>
        /// Set the value at which the the touchpad can be used as a button
        /// </summary>
        /// <param name="value"></param>
        public void SetClickThreshold(float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            touchClickThreshold = value;
        }
        /// <summary>
        /// Set the value at which the the touchpad can be used as a button
        /// </summary>
        /// <param name="value"></param>
        public void SetGripThreshold(float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            gripClickThreshold = value;
        }
    }
}
