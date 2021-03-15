using System;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Input.Input_Data
{
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
                    else if (listening && elapsedTime >= (XRInputController.GestureTimeout * Modifier)) // When the input value is false and you are waiting for a gesture
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
            public bool State(InputGesture.GestureType gestureType, XRInputController.Check check)
            {
                switch (check)
                {
                    case XRInputController.Check.Left:
                        return left.Valid(gestureType);
                    case XRInputController.Check.Right:
                        return right.Valid(gestureType);
                    case XRInputController.Check.Head:
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
            public bool State(InputGesture.GestureType gestureType, out XRInputController.Check check)
            {
                check = XRInputController.Check.Head;
                if (left.Valid(gestureType))
                {
                    check = XRInputController.Check.Left;
                    return true;
                }
                if (right.Valid(gestureType))
                {
                    check = XRInputController.Check.Right;
                    return true;
                }
                return false;
            }
        }
}