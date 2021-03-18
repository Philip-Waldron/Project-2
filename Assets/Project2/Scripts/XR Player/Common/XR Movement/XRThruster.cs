using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRThruster : XRInputAbstraction
    {
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField, Range(0, 50)] private float force;
        
        private void FixedUpdate()
        {
            Thruster(XRInputController.Check.Right);
            Thruster(XRInputController.Check.Left);
        }

        private void Thruster(XRInputController.Check check)
        {
            if (XRInputController.ControllerButton(XRInputController.XRControllerButton.Trigger, check))
            {
                playerRigidbody.AddForce(XRInputController.Forward(check) * force);
                Debug.DrawRay(XRInputController.Position(check), XRInputController.Forward(check) * force, Color.yellow);
            }
        }
    }
}
