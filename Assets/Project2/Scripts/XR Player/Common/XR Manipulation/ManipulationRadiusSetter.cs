using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace Project2.Scripts.XR_Player.Common.XR_Manipulation
{
    public class ManipulationRadiusSetter : XRInputAbstraction
    {
        public enum Direction
        {
            None,
            Vertical,
            Horizontal
        }
        [SerializeField] private XRInputController.Hand hand = XRInputController.Hand.NonDominant;
        [SerializeField, Range(0, 1)] private float modifier = .1f;
        [SerializeField] private Direction direction;

        private void Update()
        {
            switch (direction)
            {
                case Direction.Vertical when !Valid(XRInputController.Cardinal.Left, XRInputController.Cardinal.Right):
                    XRManipulationController.SetInteractionRange(XRManipulationController.GetInteractionRange() + XRInputController.AxisValue(XRInputController.CheckHand(hand)).y * modifier);
                    break;
                case Direction.Horizontal when !Valid(XRInputController.Cardinal.Forward, XRInputController.Cardinal.Back):
                    XRManipulationController.SetInteractionRange(XRManipulationController.GetInteractionRange() + XRInputController.AxisValue(XRInputController.CheckHand(hand)).x * modifier);
                    break;
                case Direction.None:
                    return;
                default:
                    return;
            }
        }
        private bool Valid(XRInputController.Cardinal a, XRInputController.Cardinal b)
        {
            return XRInputController.AxisDirection(XRInputController.CheckHand(hand), a) || XRInputController.AxisDirection(XRInputController.CheckHand(hand), b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        public void SetType(Direction method)
        {
            direction = method;
        }
    }
}
