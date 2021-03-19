using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRThruster : XRInputAbstraction
    {
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField, Range(0, 50)] private float force = 15f;
        [SerializeField, Range(1, 5)] private float duration = 3f;

        private Thruster left, right;

        private void Start()
        {
            left = gameObject.AddComponent<Thruster>();
            left.SetupThruster(XRInputController.Check.Left, playerRigidbody, force, duration);
            right = gameObject.AddComponent<Thruster>();
            right.SetupThruster(XRInputController.Check.Right, playerRigidbody, force, duration);
        }

        private void Update()
        {
            left.ThrustLogic(force, duration);
            right.ThrustLogic(force, duration);
        }
    }
}
