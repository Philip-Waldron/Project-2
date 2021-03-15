using System;
using UnityEngine;
using UnityEngine.XR;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities.Generic;
using Transition = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.InputEvents.InputEvent.Transition;

namespace Project2.Scripts.XR_Player.Common.XR_Input
{
    /// <summary>
    /// This is the central class for accessing all required input, and spatial information related to the XRPlayer,
    /// Inputs are abstracted from cardinal left and right inputs, and are referred to as DominantHand() and NonDominantHand()
    /// Whose assignment can be changed on the fly
    /// </summary>
    public class XRInputController : XRInputAbstraction
    {
        /// <summary>
        /// Your spatial reference, 
        /// </summary>
        public enum Check
        {
            Left, 
            Right, 
            Head
        }
        /// <summary>
        /// 
        /// </summary>
        public enum Hand
        {
            NonDominant, 
            Dominant
        }
        /// <summary>
        /// Work in progress
        /// </summary>
        private enum DominantHandAbstraction
        {
            XRToolkitEvents,
            TouchpadAbstraction
        }
        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float threshold = .75f;
        [SerializeField, Range(0f, 1f)] private float nibValue;
        [SerializeField, Space(10)] private DominantHandAbstraction dominantHandAbstraction = DominantHandAbstraction.XRToolkitEvents;
        [SerializeField] private TouchpadSegmentation touchpadSegmentation;
        [SerializeField] private Check dominantHand = Check.Right;
        [Header("References")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftTransform, rightTransform;
        [SerializeField] private InputDevice headXRController, leftXRController, rightXRController;
        // [SerializeField] private XRController headXRController, leftXRController, rightXRController;

        private const float AxisThreshold = .25f, AxisMinimum = 0f, AxisMaximum = 1f, ThresholdModifier = .25f, GestureTimeout = 100;
        private Transform bimanualMidpoint;

        private InputDevice HeadInputDevice => headXRController;//.inputDevice;
        private InputDevice LeftInputDevice => leftXRController;//.inputDevice;
        private InputDevice RightInputDevice => rightXRController;//.inputDevice;

        #region Events

        [HideInInspector] public InputEvents 
                    grabEvents, 
                    selectEvents, 
                    primaryEvents,
                    secondaryEvents, 
                    menuEvents,
                    axisForward,
                    axisBackward,
                    axisLeft,
                    axisRight,
                    axisCenter,
                    primaryTouchEvents,
                    analogTouchEvents,
                    analogClickEvents,
                    nibPress;
        
                [HideInInspector] public InputGestures
                    analogGestures;
        
                [HideInInspector] public ValueDeltas
                    analogDeltas,
                    interControllerDistance;

        #endregion
        
        [Serializable] public class ValueDeltas
        {
            [Serializable] public class ValueDelta
            {
                public float previousValue, currentValue, delta;

                /// <summary>
                /// 
                /// </summary>
                /// <param name="current"></param>
                /// <param name="logValue"></param>
                public void SetDelta(float current, bool logValue)
                {
                    currentValue = current;

                    if (!logValue)
                    {
                        previousValue = currentValue;
                        delta = 0f;
                    }
                    else
                    {
                        delta = currentValue - previousValue;
                        delta = delta > .75f ? 0f : delta; 
                        previousValue = currentValue;
                    }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public float GetDelta()
                {
                    return delta;
                }
            }
            private ValueDelta nonDominantValueDelta = new ValueDelta(), dominantValueDelta = new ValueDelta();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="check"></param>
            /// <returns></returns>
            public float GetDelta(Check check)
            {
                switch (check)
                {
                    case Check.Left:
                        return nonDominantValueDelta.GetDelta();
                    case Check.Right:
                        return dominantValueDelta.GetDelta();
                    case Check.Head:
                        return 0f;
                    default:
                        return 0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="dominant"></param>
            /// <param name="dominantTouch"></param>
            /// <param name="nonDominant"></param>
            /// <param name="nonDominantTouch"></param>
            public void SetDelta(float dominant, bool dominantTouch, float nonDominant, bool nonDominantTouch)
            {
                dominantValueDelta.SetDelta(dominant, dominantTouch);
                nonDominantValueDelta.SetDelta(nonDominant, nonDominantTouch);
            }
        }
        /// <summary>
        /// A generic class for taking bi-manual inputs and transitions between button presses
        /// </summary>
        [Serializable] public class InputEvents
        {
            /// <summary>
            /// Handles the logic for determining when input transitions happen
            /// </summary>
            [Serializable] public class InputEvent
            {
                public enum Transition
                {
                    Down, Up, Stay
                }
                private bool buttonDown, buttonUp, buttonStay;
                public bool current, previous;
                /// <summary>
                /// Logic for checking the state of the inputs
                /// </summary>
                /// <param name="state"></param>
                public void CheckState(bool state)
                {
                    current = state;
                    buttonDown = current && !previous;
                    buttonStay = current && previous;
                    buttonUp = !current && previous;
                    previous = current;
                }
                /// <summary>
                /// Returns the state of the input transition
                /// </summary>
                /// <param name="transition"></param>
                /// <returns></returns>
                public bool State(Transition transition)
                {
                    switch (transition)
                    {
                        case Transition.Down:
                            return buttonDown;
                        case Transition.Up:
                            return buttonUp;
                        case Transition.Stay:
                            return buttonStay;
                        default:
                            return false;
                    }
                }
            }
            public InputEvent left, right;
            /// <summary>
            /// Sets the state of the different inputs
            /// </summary>
            /// <param name="leftState"></param>
            /// <param name="rightState"></param>
            public void SetState(bool leftState, bool rightState)
            {
                left.CheckState(leftState);
                right.CheckState(rightState);
            }
            /// <summary>
            /// Returns the state of the interrogated input 
            /// </summary>
            /// <param name="check"></param>
            /// <param name="transition"></param>
            /// <returns></returns>
            public bool State(Check check, InputEvent.Transition transition)
            {
                switch (check)
                {
                    case Check.Left:
                        return left.State(transition);
                    case Check.Right:
                        return right.State(transition);
                    case Check.Head:
                        return false;
                    default:
                        return false;
                }
            }
            /// <summary>
            /// Returns the transition state of the interrogated input
            /// </summary>
            /// <param name="check"></param>
            /// <param name="transition"></param>
            /// <returns></returns>
            public bool State(Transition transition, out Check check)
            {
                if (left.State(transition))
                {
                    check = Check.Left;
                    return true;
                }
                if (right.State(transition))
                {
                    check = Check.Right;
                    return true;
                }
                check = Check.Head;
                return false;
            }
        }
        /// <summary>
        /// A generic abstraction to return gesture information about each hand
        /// </summary>
        [Serializable] public class InputGestures
        {
            private const float Modifier = .01f;
            public InputGesture left, right;
            /// <summary>
            /// A generic class for detecting gestures
            /// </summary>
            [Serializable] public class InputGesture
            {
                public enum GestureType
                {
                    SingleTap,
                    DoubleTap = 45,
                    LongPress = 50
                }
                
                private bool listening, waiting;
                private bool singleTapValid, doubleTapValid, longPressValid;
                private float startTime;

                /// <summary>
                /// Feeds in the state of the gesture inputs
                /// </summary>
                /// <param name="down"></param>
                /// <param name="up"></param>
                public void SetState(bool down, bool up)
                {
                    // Reset the state of all gestures every frame
                    ResetValidity();
                    float elapsedTime = Time.time - startTime;
                    
                    if (waiting) // If you have touch the touchpad once, but not again within the time window 
                    {
                        if (elapsedTime > (((float)GestureType.DoubleTap) * Modifier))
                        {
                            singleTapValid = true;
                            waiting = false;
                            listening = false;
                        }
                    }
                    if (listening && !waiting) // Once you have started a gesture and lift your finger
                    {
                        if (up && elapsedTime < (((float)GestureType.DoubleTap) * Modifier))
                        {
                            waiting = true;
                            return;
                        }
                        if (elapsedTime >= (((float) GestureType.LongPress) * Modifier))
                        {
                            longPressValid = true;
                            listening = false;
                            return;
                        }
                    }
                    if (down) // True the frame you touch the touchpad first
                    {
                        if (!listening) // Start listening to detect gestures, log the time this starts
                        {
                            listening = true;
                            waiting = false;
                            startTime = Time.time;
                            //Debug.Log($"Listening Started @ {startTime}");
                        }
                        else // When you press again after gesture recognition has begun
                        {
                            waiting = false;
                            if (elapsedTime <= (((float)GestureType.DoubleTap) * Modifier)) // If you have pressed again within the time limit 
                            {
                                doubleTapValid = true;
                                listening = false;
                                //Debug.Log($"Double Tap -> {elapsedTime} <= {(float)GestureType.DoubleTap * Modifier}");
                            }
                        }
                    }
                    else if (listening && elapsedTime >= (GestureTimeout * Modifier)) // When the input value is false and you are waiting for a gesture
                    {
                        //Debug.Log($"Timeout -> {elapsedTime} >= {GestureTimeout * Modifier}");
                        waiting = false;
                        listening = false;
                        ResetValidity();
                    }
                }
                /// <summary>
                /// 
                /// </summary>
                private void ResetValidity()
                {
                    singleTapValid = false;
                    doubleTapValid = false;
                    longPressValid = false;
                }
                /// <summary>
                /// Returns the validity of the input gesture
                /// </summary>
                /// <returns></returns>
                public bool Valid(GestureType gestureType)
                {
                    switch (gestureType)
                    {
                        case GestureType.DoubleTap:
                            return doubleTapValid;
                        case GestureType.LongPress:
                            return longPressValid;
                        case GestureType.SingleTap:
                            return singleTapValid;
                        default:
                            return false;
                    }
                }
            }
            /// <summary>
            /// Sets the state of the different inputs
            /// </summary>
            /// <param name="leftDownState"></param>
            /// <param name="leftUpState"></param>
            /// <param name="rightDownState"></param>
            /// <param name="rightUpState"></param>
            public void SetState(bool leftDownState, bool leftUpState, bool rightDownState, bool rightUpState)
            {
                left.SetState(leftDownState, leftUpState);
                right.SetState(rightDownState, rightUpState);
            }
            /// <summary>
            /// Returns the state of the interrogated input gesture
            /// </summary>
            /// <param name="gestureType"></param>
            /// <param name="check"></param>
            /// <returns></returns>
            public bool State(InputGesture.GestureType gestureType, Check check)
            {
                switch (check)
                {
                    case Check.Left:
                        return left.Valid(gestureType);
                    case Check.Right:
                        return right.Valid(gestureType);
                    case Check.Head:
                        return false;
                    default:
                        return false;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="gestureType"></param>
            /// <param name="check"></param>
            /// <returns></returns>
            public bool State(InputGesture.GestureType gestureType, out Check check)
            {
                check = Check.Head;
                if (left.Valid(gestureType))
                {
                    check = Check.Left;
                    return true;
                }
                if (right.Valid(gestureType))
                {
                    check = Check.Right;
                    return true;
                }
                return false;
            }
        }
        private void Awake()
        {
            bimanualMidpoint = Set.Object(gameObject, "[Bimanual Midpoint]", Vector3.zero).transform;
        }
        private void Update()
        {
            SetBimanualTransform();
            SetInputEvents();
            SetGestureEvents();
            SetValueDeltas();
        }

        private Vector3 BimanualForwardVector => Position(Check.Right) - Position(Check.Left);
        private Vector3 BimanualUpwardVector => Vector3.Lerp(Transform(Check.Right).up, Transform(Check.Left).up, .5f);
        /// <summary>
        /// 
        /// </summary>
        private void SetBimanualTransform()
        {
            // Set it to always be in the midpoint between the two controllers
            bimanualMidpoint.position = Set.MidpointPosition(Transform(DominantHand()), Transform(NonDominantHand()));
            bimanualMidpoint.ScaleFactor(InterControllerDistance());
            // Always make it align with the axis between the two controllers
            bimanualMidpoint.rotation = Quaternion.LookRotation(forward: BimanualForwardVector, upwards: BimanualUpwardVector);
            // Visualise the bimanual midpoint
            XRDebug.DrawRay(index: $"Bimanual Midpoint Forward", BimanualTransform().position, BimanualTransform().forward, .015f, Color.blue);
            XRDebug.DrawRay(index: $"Bimanual Midpoint Right", BimanualTransform().position, BimanualTransform().right, .015f, Color.red);
            XRDebug.DrawRay(index: $"Bimanual Midpoint Up", BimanualTransform().position, BimanualTransform().up, .015f, Color.green);
            // Show the inter-controller distance
            XRDebug.Log(index: $"Bimanual Controller Distance", $"\n \n \n \n[1 : {Math.Round(InterControllerDistance(), 2)}]", BimanualTransform().position, .035f);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetInputEvents()
        {
            grabEvents.SetState(leftState: Grab(Check.Left), rightState: Grab(Check.Right));
            selectEvents.SetState(leftState: Select(Check.Left), rightState: Select(Check.Right));
            primaryEvents.SetState(leftState: ControllerButton(XRControllerButton.Primary, Check.Left), rightState: ControllerButton(XRControllerButton.Primary, Check.Right)); 
            secondaryEvents.SetState(leftState: ControllerButton(XRControllerButton.Secondary, Check.Left), rightState: ControllerButton(XRControllerButton.Secondary, Check.Right)); 
            menuEvents.SetState(leftState: ControllerButton(XRControllerButton.Menu, Check.Left), rightState: ControllerButton(XRControllerButton.Menu, Check.Right)); 
            axisForward.SetState(leftState: AxisForward(InputDevice(Check.Left)), rightState: AxisForward(InputDevice(Check.Right)));
            axisBackward.SetState(leftState: AxisBack(InputDevice(Check.Left)), rightState: AxisBack(InputDevice(Check.Right)));
            axisLeft.SetState(leftState: AxisLeft(InputDevice(Check.Left)), rightState: AxisLeft(InputDevice(Check.Right)));
            axisRight.SetState(leftState: AxisRight(InputDevice(Check.Left)), rightState: AxisRight(InputDevice(Check.Right)));
            axisCenter.SetState(leftState: AxisCenter(InputDevice(Check.Left)), rightState: AxisCenter(InputDevice(Check.Right)));
            primaryTouchEvents.SetState(leftState: Touch(Check.Left, TouchType.PrimaryInput), rightState: Touch(Check.Right, TouchType.PrimaryInput));           
            analogTouchEvents.SetState(leftState: Touch(Check.Left, TouchType.TruncatedAnalogInput), rightState: Touch(Check.Right, TouchType.TruncatedAnalogInput));
            analogClickEvents.SetState(leftState: AxisClick(Check.Left), rightState: touchpadSegmentation.TouchstripClick());
            nibPress.SetState(PrimaryValue(Check.Left) >= threshold, PrimaryValue(Check.Right) >= threshold);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetGestureEvents()
        {
            analogGestures.SetState(
                leftDownState: InputEvent(Event.AnalogTouch).State(
                    Check.Left, 
                    Transition.Down),
                leftUpState: InputEvent(Event.AnalogTouch).State(
                    Check.Left, 
                    Transition.Up),
                rightDownState: InputEvent(Event.AnalogTouch).State(
                    Check.Right, 
                    Transition.Down),
                rightUpState: InputEvent(Event.AnalogTouch).State(
                    Check.Right, 
                    Transition.Up));
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetValueDeltas()
        {
            analogDeltas.SetDelta(
                dominant: AxisValue(check: DominantHand(), truncated: dominantHandAbstraction == DominantHandAbstraction.TouchpadAbstraction).y, dominantTouch: InputEvent(Event.AnalogTouch).State(DominantHand(), Transition.Stay), 
                nonDominant: AxisValue(check: NonDominantHand()).y, nonDominantTouch: true /*InputEvent(Event.AnalogTouch).State(NonDominantHand(), Transition.Stay)*/);
            interControllerDistance.SetDelta(InterControllerDistance(), true, InterControllerDistance(), true);
        }
        /// <summary>
        /// Returns the input device that is queried 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public InputDevice InputDevice(Check check)
        {
            switch (check)
            {
                case Check.Left:
                    return LeftInputDevice;
                case Check.Right:
                    return RightInputDevice;
                case Check.Head:
                    return HeadInputDevice;
                default:
                    return new InputDevice();
            }
        }
        /// <summary>
        /// Range of different input events
        /// </summary>
        public enum Event
        {
            GripPress, 
            TriggerPress,
            PrimaryTouch,
            AnalogTouch,
            AnalogClick,
            NibPress
        }
        /// <summary>
        /// An abstracted accessor to get each of the defined input events
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public InputEvents InputEvent(Event eventType)
        {
            switch (eventType)
            {
                case Event.GripPress:
                    return grabEvents;
                case Event.TriggerPress:
                    return selectEvents;
                case Event.AnalogTouch:
                    return analogTouchEvents;
                case Event.PrimaryTouch:
                    return primaryTouchEvents;
                case Event.AnalogClick:
                    return analogClickEvents;
                case Event.NibPress:
                    return nibPress;
                default:
                    return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public InputEvents InputEvent(XRControllerButton button)
        {
            switch (button)
            {
                case XRControllerButton.Grip:
                    return grabEvents;
                case XRControllerButton.Trigger:
                    return selectEvents;
                case XRControllerButton.Primary:
                    return primaryEvents;
                case XRControllerButton.Secondary:
                    return secondaryEvents;
                case XRControllerButton.Menu:
                    return menuEvents;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Input event data for axis direction
        /// </summary>
        /// <param name="cardinal"></param>
        /// <returns></returns>
        public InputEvents InputEvent(Cardinal cardinal)
        {
            switch (cardinal)
            {
                case Cardinal.Forward:
                    return axisForward;
                case Cardinal.Back:
                    return axisBackward;
                case Cardinal.Left:
                    return axisLeft;
                case Cardinal.Right:
                    return axisRight;
                case Cardinal.Center:
                    return axisCenter;
                default:
                    return null;
            }
        }
        public enum Gesture
        {
            AnalogGesture
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public InputGestures InputGesture(Gesture gesture)
        {
            switch (gesture)
            {
                case Gesture.AnalogGesture:
                    return analogGestures;
                default:
                    return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public enum Delta
        {
            PrimaryAxis,
            InterControllerDistance
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValueDeltas ValueDelta(Delta delta)
        {
            switch (delta)
            {
                case Delta.PrimaryAxis:
                    return analogDeltas;
                case Delta.InterControllerDistance:
                    return interControllerDistance;
                default:
                    return new ValueDeltas();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public Check CheckHand(Hand hand)
        {
            return hand == Hand.Dominant ? DominantHand() : NonDominantHand();
        }
        /// <summary>
        /// Returns the dominant hand
        /// </summary>
        /// <returns></returns>
        public Check DominantHand()
        {
            return dominantHand;
        }
        /// <summary>
        /// Returns the non-dominant hand
        /// </summary>
        /// <returns></returns>
        public Check NonDominantHand()
        {
            return OtherHand(DominantHand());
        }
        /// <summary>
        /// Returns the opposite hand of the one that is fed in
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static Check OtherHand(Check hand)
        {
            return hand == Check.Right ? Check.Left : Check.Right;
        }
        /// <summary>
        /// Returns true if the user's presence is detected
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool UserPresence(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.userPresence, out bool state);
            return state;
        }
        /// <summary>
        /// Returns true if the user's presence is detected
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool UserPresence(Check check)
        {
            return UserPresence(InputDevice(check));
        }
        /// <summary>
        /// Returns the transform of the left controller
        /// </summary>
        /// <returns></returns>
        private Transform LeftTransform()
        {
            return leftTransform;
        }
        /// <summary>
        /// Returns the transform of the left controller
        /// </summary>
        /// <returns></returns>
        public Transform BimanualTransform()
        {
            return bimanualMidpoint;
        }
        /// <summary>
        /// Returns the transform of the right controller
        /// </summary>
        /// <returns></returns>
        private Transform RightTransform()
        {
            return rightTransform;
        }
        /// <summary>
        /// Returns the position of the left controller
        /// </summary>
        /// <returns></returns>
        private Vector3 LeftPosition(bool local = false)
        {
            return local ? LeftTransform().localPosition : LeftTransform().position;
        }
        /// <summary>
        /// Returns the position of the right controller
        /// </summary>
        /// <returns></returns>
        private Vector3 RightPosition(bool local = false)
        {
            return local ? RightTransform().localPosition : RightTransform().position;
        }
        /// <summary>
        /// Returns the rotation of the left controller
        /// </summary>
        /// <returns></returns>
        private Vector3 LeftRotation(bool local = false)
        {
            return local ? LeftTransform().localEulerAngles : LeftTransform().eulerAngles;
        }
        /// <summary>
        /// Returns the rotation of the right controller
        /// </summary>
        /// <returns></returns>
        private Vector3 RightRotation(bool local = false)
        {
            return local ? RightTransform().localEulerAngles : RightTransform().eulerAngles;
        }
        /// <summary>
        /// Returns the distance between the two controllers
        /// </summary>
        /// <returns></returns>
        public float InterControllerDistance()
        {
            return Vector3.Distance(RightPosition(), LeftPosition());
        }
        /// <summary>
        /// Returns the transform of the HMD
        /// </summary>
        /// <returns></returns>
        private Transform HeadTransform()
        {
            return headTransform;
        }
        /// <summary>
        /// Returns the position of the HMD
        /// </summary>
        /// <returns></returns>
        private Vector3 HeadPosition(bool local = false)
        {
            return local ? headTransform.localPosition : headTransform.position;
        }
        /// <summary>
        /// Returns the rotation of the HMD
        /// </summary>
        /// <returns></returns>
        private Vector3 HeadRotation(bool local = false)
        {
            return local ? headTransform.localEulerAngles : headTransform.eulerAngles;
        }
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool Grab(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.gripButton, out bool state);
            return state;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool Select(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.triggerButton, out bool state);
            return state;
        }*/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool PrimaryTouch(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.primaryTouch, out bool state);
            return state;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool AxisTouch(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool state);
            return state;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static float GrabValue(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.grip, out float grabValue);
            return grabValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static float PrimaryValue(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.trigger, out float primaryValue);
            return primaryValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static Vector3 Velocity(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);
            return velocity;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static Vector2 AxisValue(InputDevice device)
        {
            device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value);
            return value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool AxisForward(InputDevice device)
        {
            Vector2 value = AxisValue(device);
            return  (AxisMinimum - value.x <= AxisThreshold) && (AxisMaximum - value.y <= AxisThreshold);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool AxisBack(InputDevice device)
        {
            Vector2 value = AxisValue(device);
            return  (AxisMinimum - value.x <= AxisThreshold) && (AxisMaximum + value.y <= AxisThreshold);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool AxisLeft(InputDevice device)
        {
            Vector2 value = AxisValue(device);
            return  (AxisMaximum + value.x <= AxisThreshold) && (AxisMinimum - value.y <= AxisThreshold);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool AxisCenter(InputDevice device)
        {
            Vector2 value = AxisValue(device);
            const float threshold = AxisThreshold * ThresholdModifier;
            return  (Mathf.Abs(value.x) <= threshold) && (Mathf.Abs(value.y) <= threshold);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool AxisRight(InputDevice device)
        {
            Vector2 value = AxisValue(device);
            return  (AxisMaximum - value.x <= AxisThreshold) && (AxisMinimum - value.y <= AxisThreshold);
        }
        public enum Cardinal
        {
            Forward,
            Back,
            Left,
            Right,
            Center
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2 AxisValue(Check check, bool truncated = false)
        {
            return truncated && check == DominantHand() ? touchpadSegmentation.TouchstripValue() : AxisValue(InputDevice(check));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="cardinal"></param>
        /// <returns></returns>
        public bool AxisDirection(Check check, Cardinal cardinal)
        {
            InputDevice device;
            
            switch (check)
            {
                case Check.Left:
                    device = LeftInputDevice;
                    break;
                case Check.Right:
                    device = RightInputDevice;
                    break;
                case Check.Head:
                    return false;
                default:
                    return false;
            }
            switch (cardinal)
            {
                case Cardinal.Forward:
                    return AxisForward(device);
                case Cardinal.Back:
                    return AxisBack(device);
                case Cardinal.Left:
                    return AxisLeft(device);
                case Cardinal.Right:
                    return AxisRight(device);
                case Cardinal.Center:
                    return AxisCenter(device);
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="cardinal"></param>
        /// <returns></returns>
        public bool AxisDirection(Cardinal cardinal, out Check check)
        {
            switch (cardinal)
            {
                case Cardinal.Forward when AxisForward(LeftInputDevice):
                    check = Check.Left;
                    return true;
                case Cardinal.Forward when AxisForward(RightInputDevice):
                    check = Check.Right;
                    return true;
                case Cardinal.Back when AxisBack(LeftInputDevice):
                    check = Check.Left;    
                    return true;
                case Cardinal.Back when AxisBack(RightInputDevice):
                    check = Check.Right;
                    return true;
                case Cardinal.Left when AxisLeft(LeftInputDevice):
                    check = Check.Left;
                    return true;
                case Cardinal.Left when AxisLeft(RightInputDevice):
                    check = Check.Right;
                    return true;
                case Cardinal.Right when AxisRight(LeftInputDevice):
                    check = Check.Left;
                    return true;
                case Cardinal.Right when AxisRight(RightInputDevice):
                    check = Check.Right;
                    return true;
                case Cardinal.Center when AxisCenter(LeftInputDevice):
                    check = Check.Left;
                    return true;
                case Cardinal.Center when AxisCenter(RightInputDevice):
                    check = Check.Right;
                    return true;
                default:
                    check = Check.Head;
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardinal"></param>
        /// <returns></returns>
        public bool AxisDirection(Cardinal cardinal)
        {
            switch (cardinal)
            {
                case Cardinal.Forward when AxisForward(LeftInputDevice) || AxisForward(RightInputDevice):
                    return true;
                case Cardinal.Back when AxisBack(LeftInputDevice) || AxisBack(RightInputDevice):
                    return true;
                case Cardinal.Left when AxisLeft(LeftInputDevice) || AxisLeft(RightInputDevice):
                    return true;
                case Cardinal.Right when AxisRight(LeftInputDevice) || AxisRight(RightInputDevice):
                    return true;
                case Cardinal.Center when AxisCenter(LeftInputDevice) || AxisCenter(RightInputDevice):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public Transform Transform(Check check)
        {
            switch (check)
            {
                case Check.Left:
                    return LeftTransform();
                case Check.Right:
                    return RightTransform();
                case Check.Head:
                    return HeadTransform();
                default:
                    return HeadTransform();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Transform Transform()
        {
            return transform;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool Grab(Check check)
        {
            switch (dominantHandAbstraction)
            {
                case DominantHandAbstraction.XRToolkitEvents:
                    return ControllerButton(XRControllerButton.Grip, check);
                case DominantHandAbstraction.TouchpadAbstraction when check == DominantHand():
                    return touchpadSegmentation.Grab();
                default:
                    return ControllerButton(XRControllerButton.Grip, check);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool Grab(out Check check)
        {
            if (Grab(Check.Left))
            {
                check = Check.Left;
                return true;
            }
            if (Grab(Check.Right))
            {
                check = Check.Right;
                return true;
            }
            check = Check.Head;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool Select(Check check)
        {
            switch (dominantHandAbstraction)
            {
                case DominantHandAbstraction.XRToolkitEvents:
                    return ControllerButton(XRControllerButton.Trigger, check);
                case DominantHandAbstraction.TouchpadAbstraction when check == DominantHand():
                    return touchpadSegmentation.Select();
                default:
                    return ControllerButton(XRControllerButton.Trigger, check);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool Select(out Check check)
        {
            if (Select(Check.Left))
            {
                check = Check.Left;
                return true;
            }
            if (Select(Check.Right))
            {
                check = Check.Right;
                return true;
            }
            check = Check.Head;
            return false;
        }
        public enum XRControllerButton
        {
            Grip,
            Trigger,
            Primary,
            Secondary,
            Menu
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool ControllerButton(XRControllerButton button, Check check)
        {
            switch (button)
            {
                case XRControllerButton.Primary:
                    InputDevice(check).TryGetFeatureValue(CommonUsages.primaryButton, out bool primary);
                    return primary;
                case XRControllerButton.Secondary:
                    InputDevice(check).TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondary);
                    return secondary;
                case XRControllerButton.Menu:
                    InputDevice(check).TryGetFeatureValue(CommonUsages.menuButton, out bool menu);
                    return menu;
                case XRControllerButton.Grip:
                    InputDevice(check).TryGetFeatureValue(CommonUsages.gripButton, out bool grip);
                    return grip;
                case XRControllerButton.Trigger:
                    InputDevice(check).TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);
                    return trigger;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool AxisClick(Check check)
        {
            InputDevice(check).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool valid);
            return valid;
        }
        public enum TouchType
        {
            PrimaryInput,
            AnalogInput,
            TruncatedAnalogInput
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="touchType"></param>
        /// <returns></returns>
        public bool Touch(Check check, TouchType touchType)
        {
            switch (touchType)
            {
                case TouchType.PrimaryInput:
                    return PrimaryTouch(InputDevice(check));
                case TouchType.AnalogInput:
                    return AxisTouch(InputDevice(check));
                case TouchType.TruncatedAnalogInput:
                    return check == DominantHand() ? TouchpadSegmentation().TouchstripTouch() : AxisTouch(InputDevice(check));
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public float GrabValue(Check check)
        {
            return GrabValue(InputDevice(check));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public float PrimaryValue(Check check)
        {
            return PrimaryValue(InputDevice(check));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public Vector3 Velocity(Check check)
        {
            return Velocity(InputDevice(check));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="flatten"></param>
        /// <returns></returns>
        public Vector3 Forward(Check check, bool flatten = false)
        {
            return flatten ? Vector3.ProjectOnPlane(Transform(check).forward, planeNormal: Vector3.up).normalized : Transform(check).forward;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public Vector2 FlattenedForward(Check check)
        {
            Vector3 flattened = Forward(check, flatten: true);
            return new Vector2(flattened.x, flattened.z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public Vector3 Position(Check check, bool local = false)
        {
            switch (check)
            {
                case Check.Left:
                    return LeftPosition(local);
                case Check.Right:
                    return RightPosition(local);
                case Check.Head:
                    return HeadPosition(local);
                default:
                    return Vector3.zero;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public Vector3 Rotation(Check check, bool local = false)
        {
            switch (check)
            {
                case Check.Left:
                    return LeftRotation(local);
                case Check.Right:
                    return RightRotation(local);
                case Check.Head:
                    return HeadRotation(local);
                default:
                    return Vector3.zero;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public Vector3 NormalisedPosition(Check check, bool local = false)
        {
            return new Vector3(Position(check, local).x, 0, Position(check, local).z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public Vector3 NormalisedRotation(Check check, bool local = false)
        {
            return new Vector3(0, Rotation(check, local).y, 0f);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TouchpadSegmentation TouchpadSegmentation()
        {
            return touchpadSegmentation;
        }
    }
}