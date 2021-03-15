using System;
using Project2.Scripts.XR_Player.Common.XR_Input.Input_Data;
using Project2.Scripts.XR_Player.Common.XR_Interaction;
using UnityEngine;
using XR_Prototyping.Scripts.Common;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interaction;
using XR_Prototyping.Scripts.Common.XR_Interface;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;
using Event = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Event;
using Information = XR_Prototyping.Scripts.Utilities.IndirectInteraction.InteractionInformation;

namespace Project2.Scripts.XR_Player.Common.Elements
{
    [Serializable] public class InteractionElement : XRInputAbstraction
    {
        public Check check;
        public Transform proxy;
        
        [HideInInspector] public LineRenderer line;
        [HideInInspector] public XRInterfaceAbstraction currentXRInterface, previousXRInterface;

        private NearInteractionVisual nearInteractionVisual;

        public bool Initialised { get; private set; }
        public bool Allowed  { get; set; }
        public bool Engaged  { get; set; }
        public Information Information { get; set; }
        
        private DirectInteraction directInteraction;
        private XRInteractionOrigin interactionOrigin;

        private void Update()
        {
            SetTransform();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handedness"></param>
        /// <param name="proxyTransform"></param>
        /// <param name="material"></param>
        /// <param name="width"></param>
        /// <param name="nearVisual"></param>
        public void InitialiseInteractionElement(Check handedness, Transform proxyTransform, Material material, float width, GameObject nearVisual)
        {
            check = handedness;
            proxy = proxyTransform;
            line = proxy.gameObject.Line(material, startWidth: width, endWidth: width * .1f, startEnabled: false, worldSpace: true);
            directInteraction = XRInputController.Transform(check).gameObject.AddComponent<DirectInteraction>();
            directInteraction.SetupDirectInteraction(check);
            Allowed = true;
            GameObject visual = Instantiate(nearVisual, transform);
            visual.name = $"[Near Visual] {check}";
            nearInteractionVisual = visual.GetComponent<NearInteractionVisual>();
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetTransform()
        {
            proxy.Transforms(follow: XRInputController.Transform(check));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetLineState(bool state)
        {
            line.enabled = state;
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetDefaultState()
        {
            Information = new Information();
            SetLineState(false);
            nearInteractionVisual.SetVisualState(false);
            currentXRInterface = null;
            Engaged = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void DrawLine(Vector3 target)
        {
            if (!line.enabled)
            {
                SetLineState(true);
            }
            line.DrawLine(proxy.position, target);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        public void SetInteractionOrigin(XRInteractionOrigin origin)
        {
            interactionOrigin = origin;
            Initialised = true;
        }
        /// <summary>
        /// 
        /// </summary>
        public XRInteractionOrigin GetInteractionOrigin()
        {
            return interactionOrigin;
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetNearVisualState(bool state, Information information)
        {
            nearInteractionVisual.SetVisualState(state);
            nearInteractionVisual.SetVisual(information);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interactionElement"></param>
        /// <param name="information"></param>
        public static void InteractionLogic(InteractionElement interactionElement, Information information)
        {
            // Set the current interface reference
            interactionElement.currentXRInterface = information.currentXRInterface;
            
            switch (XRInteractionController.NearInteraction(information.interactionDistance))
            {
                // This is a near field interaction
                case true:
                    interactionElement.line.enabled = false;
                    interactionElement.SetNearVisualState(true, information);
                    break;
                // This is an indirect interaction
                case false:
                    interactionElement.line.enabled = true;
                    interactionElement.DrawLine(information.hit.point);
                    interactionElement.SetNearVisualState(false, information);
                    break;
            }

            // Conduct the logic for determining interaction states
            if (XRInputController.InputEvent(Event.TriggerPress).State(interactionElement.check, InputEvents.InputEvent.Transition.Down))
            {
                information.currentXRInterface.SelectStart(interactionElement.check);
            }
            if (XRInputController.InputEvent(Event.TriggerPress).State(interactionElement.check, InputEvents.InputEvent.Transition.Stay))
            {
                information.currentXRInterface.SelectStay(interactionElement.check);
            }
            if (XRInputController.InputEvent(Event.TriggerPress).State(interactionElement.check, InputEvents.InputEvent.Transition.Up))
            {
                information.currentXRInterface.SelectEnd(interactionElement.check);
            }
        }
    }
}