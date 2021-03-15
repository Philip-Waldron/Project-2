using Project2.Scripts.XR_Player.Common.XR_Input.Input_Data;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu;
using XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu.Menu_Type;
using XR_Prototyping.Scripts.Utilities.Generic;
using Event =  Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Event;
using Gesture =  Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Gesture;

namespace Project2.Scripts.XR_Player.Common.XR_Interface.XR_Menu
{
    public class SummonedMenuController : XRMenu
    {
        [SerializeField] private Transform lookTarget;
        
        private bool triggerEnd;

        protected override void MenuStart()
        {

        }
        protected override void MenuAwake()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuUpdate()
        {
            if (menuType.triggerType == MenuType.TriggerType.PointAt)
            {
                XRDebug.DrawRay($"{name} Look Ray", lookTarget.position, lookTarget.forward, .01f, Color.red);
                XRDebug.DrawSphere($"{name} Look Location", lookTarget.position, .01f, Color.red);
            }

            if (latch && TriggerEnd())
            {
                MenuSummonEnd();
            }
            else if (TriggerStart())
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
                    return valid && XRInputController.InputEvent(menuType.controllerButton).State(XRInputController.CheckHand(menuType.triggerHand), InputEvents.InputEvent.Transition.Up);
                case MenuType.TriggerType.PointAt:
                    return valid && XRInputController.Forward(menuType.pointTrigger).PointingAt(lookTarget.forward, menuType.threshold);
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool TriggerEnd()
        {
            switch (menuType.triggerType)
            {
                case MenuType.TriggerType.PointAt:
                    return !XRInputController.Forward(menuType.pointTrigger).PointingAt(lookTarget.forward, menuType.threshold);
                default:
                    return triggerEnd || TriggerStart();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void CloseMenu()
        {
            triggerEnd = true;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuSummonStart()
        {
            latch = true;
            SetMenuPosition();
            SetMenuState(true);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuSummonStay()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuSummonEnd()
        {
            ResetMenu();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void ResetMenu()
        {
            latch = false;
            triggerEnd = false;
            SetMenuState(false);
        }
    }
}
