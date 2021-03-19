using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class Thruster : XRInputAbstraction
    { 
        [Range(0, 1)] public float value;
        public XRInputController.Check check;
        
        private Rigidbody playerRigidbody;
        private float force = 1f, duration = 1f;
        private bool thrusting;
        private float thrustStartTime;

        private Vector3 ThrustVector => (-XRInputController.Forward(check)) * force;

        private bool TimeOut()
        {
            bool timedOut = Value() >= 1f;
            if (timedOut)
            {
                Debug.Log($"Thrusting timed out at {Time.time}");
            }
            return timedOut;
        }

        private float Value()
        {
            if (duration == 0) 
            {
                Debug.Log("Duration is 0, stop trying to divide by it!!");
                value = 0f;
                return value;
            }

            value = (Time.time - thrustStartTime) / duration;
            value = Mathf.Clamp(value, 0f, 1f);
            return value;
        }
        
        public void SetupThruster(XRInputController.Check setCheck, Rigidbody setRigidbody, float setForce, float setDuration)
        {
            check = setCheck;
            playerRigidbody = setRigidbody;
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
                if (TimeOut() || ThrustEnd())
                {
                    thrusting = false;
                }
            }
            if (ThrustStart())
            {
                thrustStartTime = Time.time;
                thrusting = true;
                Debug.Log($"Thrusting started at {thrustStartTime}");
            }
        }

        private void Thrust()
        {
            playerRigidbody.AddForce(ThrustVector);
            Debug.DrawRay(XRInputController.Position(check), ThrustVector, Color.Lerp(Color.white, Color.clear, Value()));
        }

        private bool ThrustStart()
        {
            return !thrusting && XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Down);
        }

        private bool ThrustEnd()
        {
            return thrusting && XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Up);
        }
    }
}