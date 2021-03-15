using System;

namespace Project2.Scripts.XR_Player.Common.XR_Input.Input_Data
{
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
            public bool State(XRInputController.Check check, InputEvent.Transition transition)
            {
                switch (check)
                {
                    case XRInputController.Check.Left:
                        return left.State(transition);
                    case XRInputController.Check.Right:
                        return right.State(transition);
                    case XRInputController.Check.Head:
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
            public bool State(InputEvent.Transition transition, out XRInputController.Check check)
            {
                if (left.State(transition))
                {
                    check = XRInputController.Check.Left;
                    return true;
                }
                if (right.State(transition))
                {
                    check = XRInputController.Check.Right;
                    return true;
                }
                check = XRInputController.Check.Head;
                return false;
            }
        }
}