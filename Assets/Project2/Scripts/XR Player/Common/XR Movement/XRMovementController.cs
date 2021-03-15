using System;
using System.Collections.Generic;
using System.Linq;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Input.Input_Data;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRMovementController : XRInputAbstraction
    {
        [Header("Movement Settings")]
        [SerializeField, Range(0f, 100f)] private float force;
        [SerializeField, Range(0f, 1f)] private float hipOffset = .15f, headOffset = .75f;
        [SerializeField, Range(float.Epsilon, .01f)] private float connectionWidth;
        [SerializeField] private XRInputController.XRControllerButton attach = XRInputController.XRControllerButton.Grip, move = XRInputController.XRControllerButton.Trigger;
        [Header("Movement References")] 
        [SerializeField] private Material connectionMaterial;
        
        private GameObject movementParent;
        private XRMovementInformation left, right;

        private Rigidbody PlayerRigidbody => GetComponent<Rigidbody>();

        private void Awake()
        {
            movementParent = Set.Object(gameObject, "[Movement Parent]", Vector3.zero);
            left = gameObject.AddComponent<XRMovementInformation>();
            right = gameObject.AddComponent<XRMovementInformation>();
            left.SetupMovementInformation(movementParent,  XRInputController.Check.Left, connectionMaterial, connectionWidth, PlayerRigidbody);
            right.SetupMovementInformation(movementParent, XRInputController.Check.Right, connectionMaterial, connectionWidth, PlayerRigidbody);
        }

        private void Update()
        {
            SetTransforms();
            FindValidAnchors();
        }

        private void FixedUpdate()
        {
            CheckStates();
        }

        private void SetTransforms()
        {
            movementParent.transform.position = XRInputController.NormalisedPosition(XRInputController.Check.Head);
            movementParent.transform.eulerAngles = XRInputController.NormalisedRotation(XRInputController.Check.Head);
            left.SetTransform(-hipOffset, heightOffset: headOffset);
            right.SetTransform(hipOffset, heightOffset: headOffset);
        }

        private void FindValidAnchors()
        {
            FindValidAnchor(left);
            FindValidAnchor(right);
        }

        private void CheckStates()
        {
            AttachDetach(left);
            AttachDetach(right);
            
            MoveToAnchor(left);
            MoveToAnchor(right);
        }

        private static void FindValidAnchor(XRMovementInformation movementInformation)
        {
            if (Physics.Raycast(movementInformation.Origin, movementInformation.CastVector, out RaycastHit hit) && hit.transform.CompareTag("CanAttach"))
            {
                movementInformation.SetAttachPoint(hit);    
            }
            else
            {
                movementInformation.ClearAttachPoint();
            }
            
            movementInformation.AttachVisual();
        }

        private void AttachDetach(XRMovementInformation movementInformation)
        { 
            if (movementInformation.Attached && XRInputController.InputEvent(attach).State(movementInformation.check, XRInputController.InputEvents.InputEvent.Transition.Up))
            {
                movementInformation.Detach();
            }
            if (XRInputController.InputEvent(attach).State(movementInformation.check, XRInputController.InputEvents.InputEvent.Transition.Down))
            {
                movementInformation.Attach();
            }
        }
        
        private void MoveToAnchor(XRMovementInformation movementInformation)
        {
            if (movementInformation.Attached && XRInputController.ControllerButton(move, movementInformation.check))
            {
                PlayerRigidbody.AddForce(movementInformation.MoveVector * force, ForceMode.Acceleration);
                Debug.DrawRay(movementInformation.Origin, movementInformation.MoveVector * force, Color.red);
            }
        }
    }
}
