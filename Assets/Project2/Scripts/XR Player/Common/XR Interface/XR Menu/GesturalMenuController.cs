using System.Collections.Generic;
using System.Linq;
using Project2.Scripts.XR_Player.Common.XR_Input.Input_Data;
using Project2.Scripts.XR_Player.Common.XR_Interaction;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Interaction;
using XR_Prototyping.Scripts.Common.XR_Interface;
using XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu;
using XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu.Menu_Type;
using XR_Prototyping.Scripts.Utilities.Generic;
using Event =  Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Event;
using Gesture =  Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Gesture;

namespace Project2.Scripts.XR_Player.Common.XR_Interface.XR_Menu
{
    public class GesturalMenuController : XRMenu
    {
        private enum GesturalMenuBehaviour
        {
            OrientatedAtCenterPoint,
            DefaultAlignment
        }
        [Header("Gestural Menu Settings")] 
        [SerializeField] private Transform interfaceCenterPoint;
        [SerializeField, Range(0f, 1f)] private float radius;
        [SerializeField] private GesturalMenuBehaviour gesturalMenuBehaviour;
        [SerializeField] private List<XRInterfaceAbstraction> gesturalMenuInterfaceElements;
        [SerializeField, Range(0f, .25f)] private float maximumInteractionDistance = .1f;
        
        private XRInterfaceAbstraction currentElement, previousElement;
        private List<Transform> MenuElements => gesturalMenuInterfaceElements.Select(element => element.transform).ToList();

        protected override void MenuAwake() { }
        protected override void MenuStart()
        {
            interfaceElements = gesturalMenuInterfaceElements.ToArray();
            SetMenuState(menuType.triggerType == MenuType.TriggerType.Persistent);
            OrientateMenu();
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuUpdate()
        {
            if (latch)
            {
                MenuSummonStay();
                if (TriggerEnd())
                {
                    MenuSummonEnd();
                }
                return;
            }
            if (TriggerStart())
            {
                MenuSummonStart();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool TriggerStart()
        {
            bool valid = XRInteractionController.QueryInteractionAvailability(XRInputController.CheckHand(menuType.triggerHand));
            switch (menuType.triggerType)
            {
                case MenuType.TriggerType.DoubleTap:
                    return valid && XRInputController.InputGesture(Gesture.AnalogGesture).State(gestureType: InputGestures.InputGesture.GestureType.DoubleTap, check: XRInputController.CheckHand(menuType.triggerHand));
                case MenuType.TriggerType.SingleTap:
                    return valid && XRInputController.InputGesture(Gesture.AnalogGesture).State(gestureType: InputGestures.InputGesture.GestureType.SingleTap, check: XRInputController.CheckHand(menuType.triggerHand));
                case MenuType.TriggerType.LongPress:
                    return valid && XRInputController.InputGesture(Gesture.AnalogGesture).State(gestureType: InputGestures.InputGesture.GestureType.LongPress, check: XRInputController.CheckHand(menuType.triggerHand));
                case MenuType.TriggerType.ControllerButton:
                    return valid && XRInputController.InputEvent(menuType.controllerButton).State(XRInputController.CheckHand(menuType.triggerHand), InputEvents.InputEvent.Transition.Down);
                default:
                    return false;
            }
        }

        protected override bool TriggerEnd()
        {
            return XRInputController.InputEvent(Event.AnalogTouch).State(check: XRInputController.CheckHand(menuType.triggerHand), transition: InputEvents.InputEvent.Transition.Up);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuSummonStart()
        {
            SetMenuPosition();
            SetMenuState(true);
            
            latch = true;
            currentElement = null;
            previousElement = null;
            XRInteractionController.SetAllowedState(XRInputController.CheckHand(menuType.triggerHand), false);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuSummonStay()
        {
            FindCurrentInterfaceElement();
            XRInteractionController.CheckState(XRInputController.CheckHand(menuType.triggerHand), currentElement, previousElement);
            previousElement = currentElement;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuSummonEnd()
        {
            // Call selection event
            if (currentElement != null)
            {
                currentElement.SelectEnd(XRInputController.CheckHand(menuType.triggerHand), immediate: true);
            }
            // Restore its normal state
            ResetMenu();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void ResetMenu()
        {
            if (currentElement != null)
            {
                currentElement.EngageEnd(XRInputController.CheckHand(menuType.triggerHand), immediate: true);
            }
            XRInteractionController.SetAllowedState(XRInputController.CheckHand(menuType.triggerHand), true);
            latch = false;
            SetMenuState(false);
        }
        /// <summary>
        /// Returns the closest of the buttons in the gestural menu
        /// </summary>
        /// <returns></returns>
        private void FindCurrentInterfaceElement()
        {
            bool valid = false;
            float smallestDistance = float.PositiveInfinity;
            Vector3 position = XRInputController.Position(XRInputController.DominantHand()), closest = Vector3.zero;
            foreach (XRInterfaceAbstraction element in interfaceElements)
            {
                Vector3 closestPoint = element.Collider.ClosestPoint(position);
                float distance = Vector3.Distance(closestPoint, position);
                if (distance < smallestDistance && distance < maximumInteractionDistance)
                {
                    closest = closestPoint;
                    currentElement = element;
                    smallestDistance = distance;
                    valid = true;
                }
            }
            currentElement = valid ? currentElement : null;
            XRDebug.DrawLine($"{GetInstanceID()} Gestural Menu Closest Element", position, closest, Color.black);
        }
        
        private void OnValidate()
        {
            OrientateMenu();
        }
        /// <summary>
        /// 
        /// </summary>
        private void OrientateMenu()
        {
            if (gesturalMenuBehaviour == GesturalMenuBehaviour.OrientatedAtCenterPoint && interfaceCenterPoint != null)
            {
                interfaceCenterPoint.transform.localPosition = new Vector3(0f, 0f, -radius);
                MenuElements.LookAt(interfaceCenterPoint, lookAway: true);
            }
        }
    }
}
