using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRThruster : XRInputAbstraction
    {
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField, Range(0, 50)] private float force;
        [SerializeField, Range(1, 5)] private float duration;

        private Thruster left, right;
        
        [Serializable] public class Thruster
        {
            private XRInputController.Check check;
            private Rigidbody rigidbody;
            private float force, duration;
            private bool thrusting;

            private float thrustStartTime;
            private bool TimeOut => Time.time - thrustStartTime >= duration;
            private float Value => (Time.time - thrustStartTime) / duration;
            
            public void SetupThruster(XRInputController.Check setCheck, Rigidbody setRigidbody, float setForce, float setDuration)
            {
                check = setCheck;
                rigidbody = setRigidbody;
                force = setForce;
                duration = setDuration;
            }
            
            public void ThrustLogic(float setForce, float setDuration)
            {
                force = setForce;
                duration = setDuration;
                
                if (thrusting)
                {
                    Thrust();
                    if (TimeOut || ThrustEnd())
                    {
                        thrusting = false;
                    }
                }
                if (ThrustStart())
                {
                    thrustStartTime = Time.time;
                    thrusting = true;
                }
            }

            private void Thrust()
            {
                rigidbody.AddForce(XRInputController.Forward(check) * force);
                Debug.DrawRay(XRInputController.Position(check), XRInputController.Forward(check) * force, Color.yellow);
                Debug.DrawRay(XRInputController.Position(check), -XRInputController.Forward(check) * (1 - Value), Color.green);
            }

            private bool ThrustStart()
            {
                return !thrusting && XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Down);
            }
            
            private bool ThrustEnd()
            {
                return (thrusting && XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Up));
            }
        }

        private void Awake()
        {
            left = new Thruster();
            left.SetupThruster(XRInputController.Check.Left, playerRigidbody, force, duration);
            right = new Thruster();
            right.SetupThruster(XRInputController.Check.Right, playerRigidbody, force, duration);
        }

        private void FixedUpdate()
        {
            left.ThrustLogic(force, duration);
            right.ThrustLogic(force, duration);
        }
    }
}
