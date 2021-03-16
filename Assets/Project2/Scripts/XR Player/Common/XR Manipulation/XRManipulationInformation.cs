using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Manipulation
{
    public class XRManipulationInformation : XRInputAbstraction
    {
        private bool grabbed, gravity;
        private Transform lasso;
        private Transform grabbedObject;
        private Outline grabbedOutline;
        private Rigidbody grabbedRigidbody;
        private XRInputController.Check check;

        private XRManipulationController manipulationController;

        public void SetupManipulation(XRManipulationController controller, XRInputController.Check set)
        {
            check = set;
            manipulationController = controller;
            lasso = Set.Object(XRInputController.Transform(check).gameObject, $"[Lasso] {set.ToString()}", new Vector3(0f, 0f, manipulationController.lassoOffset)).transform;
        }

        public void ManipulationLogic()
        {
            if (grabbed)
            {
                grabbedObject.position = Vector3.Lerp(grabbedObject.position, lasso.position, .2f);
                    
                if (XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Up))
                {
                    grabbed = false;
                    grabbedRigidbody.AddForce(XRInputController.Forward(check) * XRInputController.Velocity(check).magnitude, ForceMode.Impulse);
                    grabbedRigidbody.useGravity = gravity;
                }
            }
            if (Physics.Raycast(XRInputController.Position(check), XRInputController.Forward(check), out RaycastHit hit))
            {
                if (hit.transform.CompareTag("CanGrab"))
                {
                    if (XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Down))
                    {
                        grabbed = true;
                        grabbedObject = hit.transform;
                        grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
                        gravity = grabbedRigidbody.useGravity;
                        grabbedRigidbody.useGravity = false;
                    }
                }
            }
        }
    }
}