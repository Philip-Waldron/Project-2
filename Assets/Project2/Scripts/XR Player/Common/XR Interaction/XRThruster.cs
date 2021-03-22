using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.VFX;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRThruster : MonoBehaviour
    { 
        [Range(0, 1)] public float cooldownProgress;
        public XRInputController.Check check;

        private XRThrusterController thrusterController;
        private ThrusterCooldownVisual cooldownVisual;
        private VisualEffect thrustEffect;
        private GameObject thruster;
        private Rigidbody playerRigidbody;
        private float force, cooldown = 1f;
        private float lastThrustTime;

        private Vector3 ThrustVector => (- XRInputController.Instance.Forward(check)) * force;

        private bool Cooldown()
        {
            bool coolingDown = CooldownValue() < 1f;
            // Debug.Log(coolingDown ? $"{check} thruster cooling down!" : $"{check} thruster cooled down at {Time.time}");
            return coolingDown;
        }

        private float CooldownValue()
        {
            if (cooldown == 0) 
            {
                Debug.Log("Cooldown duration is 0, stop trying to divide by it!!");
                cooldownProgress = 1f;
            }
            else
            {
                cooldownProgress = (Time.time - lastThrustTime) / cooldown;
                cooldownProgress = Mathf.Clamp(cooldownProgress, 0f, 1f);
            }

            if (cooldownProgress > .25f)
            {
                thrustEffect.SendEvent("ThrustEnd");
            }
            
            cooldownVisual.SetCooldown(cooldownProgress);
            return cooldownProgress;
        }
        
        public void SetupThruster(XRThrusterController controller, XRInputController.Check setCheck, Rigidbody setRigidbody)
        {
            thrusterController = controller;
            check = setCheck;
            playerRigidbody = setRigidbody;
            thruster = Instantiate(thrusterController.thruster, XRInputController.Instance.Transform(check));
            cooldownVisual = thruster.GetComponentInChildren<ThrusterCooldownVisual>();
            thrustEffect = thruster.GetComponent<VisualEffect>();
        }
        
        public void ThrustLogic(float setForce, float setCooldown)
        {
            force = setForce;
            cooldown = setCooldown;

            if (!TriggerThrust()) return;
            lastThrustTime = Time.time;
            Debug.Log($"{check} thrust at {lastThrustTime}");
            Thrust();
        }

        /// <summary>
        /// Apply the force to the player in the defined direction
        /// </summary>
        private void Thrust()
        {
            playerRigidbody.AddForce(ThrustVector, ForceMode.Impulse);
            thrustEffect.SendEvent("ThrustStart");
            thrustEffect.SetVector3($"Thrust Direction", XRInputController.Instance.Forward(check));
        }
        
        /// <summary>
        /// What triggers the start of the thrust, will always return false when there is a cooldown
        /// </summary>
        /// <returns></returns>
        private bool TriggerThrust()
        {
            return !Cooldown() && XRInputController.Instance.InputEvent(XRInputController.XRControllerButton.Trigger).State(check, XRInputController.InputEvents.InputEvent.Transition.Down);
        }
    }
}