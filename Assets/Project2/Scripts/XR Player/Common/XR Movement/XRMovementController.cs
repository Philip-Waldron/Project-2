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
        [Serializable] public struct MovementInformation
        {
            public XRInputController.Check check;
            public LineRenderer connection;

            private RaycastHit validPoint, attachedPoint;
            
            private Transform origin, hit, midpoint;

            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 Origin => origin.position;
            public Vector3 CastVector => origin.forward;
            public Vector3 MoveVector => (hit.transform.position - Origin).normalized;

            public bool Attached { get; set; }
            private HingeJoint hingeJoint;

            public void SetupMovementInformation(GameObject parent, XRInputController.Check set, float offset, Material material, float width, Rigidbody player)
            {
                check = set;
                origin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: Vector3.zero).transform;
                hit = Set.Object(parent, $"[Movement Hit] {set.ToString()}", position: Vector3.zero).transform;
                midpoint = Set.Object(parent, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                
                connection = origin.gameObject.Line(material, width);
            }

            public void SetTransform(float hipOffset, float heightOffset)
            {
                origin.LookAt(ControllerPosition);
                origin.localPosition = new Vector3(hipOffset, 0f, -.01f);
                Vector3 position = origin.position;
                position = new Vector3(position.x, XRInputController.Position(XRInputController.Check.Head).y - heightOffset, position.z);
                origin.position = position;
                midpoint.LerpMidpoint(origin, hit, .3f);
            }

            public void SetAttachPoint(RaycastHit raycastHit)
            {
                validPoint = raycastHit;
                if (Attached) return;
                hit.position = Vector3.Lerp(hit.position, raycastHit.point, .5f);
            }

            public void ClearAttachPoint()
            {
                hit.position = ControllerPosition;
            }

            public void AttachVisual()
            {
                connection.BezierLine(origin.position, midpoint.position, hit.position);
            }

            public void Attach()
            {
                Attached = true;
                attachedPoint = validPoint;
            }

            public void Detach()
            {
                Attached = false;
            }
        }

        [Header("Movement Settings")]
        [SerializeField, Range(0f, 10f)] private float force;
        [SerializeField, Range(0f, 1f)] private float hipOffset = .15f, headOffset = .75f;
        [SerializeField, Range(float.Epsilon, .01f)] private float connectionWidth;
        [Header("Movement References")] 
        [SerializeField] private Material connectionMaterial;
        
        private GameObject movementParent;
        private MovementInformation left, right;

        private Rigidbody PlayerRigidbody => GetComponent<Rigidbody>();

        private void Awake()
        {
            movementParent = Set.Object(gameObject, "[Movement Parent]", Vector3.zero);
            left.SetupMovementInformation(movementParent, XRInputController.Check.Left, - hipOffset, connectionMaterial, connectionWidth, PlayerRigidbody);
            right.SetupMovementInformation(movementParent, XRInputController.Check.Right, hipOffset, connectionMaterial, connectionWidth, PlayerRigidbody);
        }

        private void Update()
        {
            SetTransforms();
            FindValidAnchors();
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

        private static void FindValidAnchor(MovementInformation movementInformation)
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

        private static void AttachDetach(MovementInformation movementInformation)
        {
            if (XRInputController.InputEvent(XRInputController.XRControllerButton.Grip).State(movementInformation.check, InputEvents.InputEvent.Transition.Down))
            {
                movementInformation.Attach();
            }
            if (XRInputController.InputEvent(XRInputController.XRControllerButton.Grip).State(movementInformation.check, InputEvents.InputEvent.Transition.Up))
            {
                movementInformation.Detach();
            }
        }
        
        private void MoveToAnchor(MovementInformation movementInformation)
        {
            if (!movementInformation.Attached) return;
            
            if (XRInputController.InputEvent(XRInputController.XRControllerButton.Trigger).State(movementInformation.check, InputEvents.InputEvent.Transition.Stay))
            {
                PlayerRigidbody.AddForce(movementInformation.MoveVector * force, ForceMode.Acceleration);
            }
        }
    }
}
