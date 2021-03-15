using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu.Menu_Type
{
    [CreateAssetMenu(fileName = "Menu Type", menuName = "XR Prototyping/Menu Type", order = 0)]
    public class MenuType : ScriptableObject
    {
        public enum MovementBehaviour
        {
            Attached, 
            Static,
            RubberBanded
        }
        public enum TriggerType
        {
            Persistent,
            PointAt,
            DoubleTap,
            LongPress,
            SingleTap,
            ControllerButton
        }
        public enum MenuOrientation
        {
            FullyAligned,
            VerticallyAligned,
            Vertical
        }
        [Header("XR Menu Configuration")]
        public TriggerType triggerType;
        public MovementBehaviour movementBehaviour = MovementBehaviour.Static;
        public XRInputController.Hand attachedHand = XRInputController.Hand.Dominant, triggerHand = XRInputController.Hand.Dominant;
        public XRInputController.XRControllerButton controllerButton = XRInputController.XRControllerButton.Menu;
        public MenuOrientation menuOrientation = MenuOrientation.VerticallyAligned;
        public XRInputController.Check pointTrigger = XRInputController.Check.Head;
        [Range(float.Epsilon, 90f)] public float threshold = 30f;
    }
}