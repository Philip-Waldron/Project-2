using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRThrusterController : XRInputAbstraction
    {
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField, Range(0, 150)] private float force = 50f;
        [SerializeField, Range(1, 5)] private float cooldown = 3f;
        public GameObject thruster;

        private XRThruster left, right;

        private void Start()
        {
            left = gameObject.AddComponent<XRThruster>();
            left.SetupThruster(this, XRInputController.Check.Left, playerRigidbody);
            right = gameObject.AddComponent<XRThruster>();
            right.SetupThruster(this, XRInputController.Check.Right, playerRigidbody);
        }

        private void Update()
        {
            left.ThrustLogic(force, cooldown);
            right.ThrustLogic(force, cooldown);
        }
    }
}
