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
            public XRAttachableObject attachableObject;
            public LineRenderer connection;

            private Rigidbody playerRigidbody;
            private Transform origin, hit, midpoint;

            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 Origin => origin.position;
            public Vector3 CastVector => origin.forward;

            private bool attached, valid;
            
            private HingeJoint hingeJoint;

            public void SetupMovementInformation(GameObject parent, XRInputController.Check set, float offset, Material material, float width, Rigidbody player)
            {
                playerRigidbody = player;
                
                check = set;
                origin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: new Vector3(offset, 1f, -.01f)).transform;
                hit = Set.Object(parent, $"[Movement Hit] {set.ToString()}", position: Vector3.zero).transform;
                midpoint = Set.Object(parent, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                
                connection = origin.gameObject.Line(material, width);
            }

            public void SetTransform()
            {
                origin.LookAt(ControllerPosition);
                midpoint.LerpMidpoint(origin, hit, .5f);

                if (!XRDebug.Enabled) return;
                XRDebug.DrawRay($"{check.ToString()} Movement Origin Forward", Origin, origin.forward, 1f, Color.red);
                XRDebug.DrawRay($"{check.ToString()} Movement Origin Forward", hit.position, Vector3.up, .1f, Color.blue);
                XRDebug.DrawRay($"{check.ToString()} Movement Origin Forward", midpoint.position, Vector3.up, .1f, Color.yellow);
            }

            public void SetAttachPoint(XRAttachableObject attachable, RaycastHit raycastHit, bool state)
            {
                if (attached) return;
                
                valid = state;
                
                hit.position = Vector3.Lerp(hit.position, valid ? raycastHit.point : ControllerPosition, .5f);
                attachableObject = valid ? attachable : null;
                
                Debug.Log($"{check.ToString()} Movement → {(valid ? attachable.name : "No Valid Attach Point")}");
            }

            public void AttachVisual()
            {
                connection.BezierLine(origin.position, midpoint.position, hit.position);
            }

            public void Attach()
            {
                attached = true;
                hingeJoint = origin.gameObject.AddComponent<HingeJoint>();
                //hingeJoint.anchor = hit.position;
                hingeJoint.connectedBody = playerRigidbody;
            }

            public void Detach()
            {
                attached = false;
                Destroy(hingeJoint);
            }
        }

        [Header("Movement Settings")]
        [SerializeField, Range(0f, 3f)] private float castRadius;
        [SerializeField, Range(0f, 100f)] private float castDistance;
        [SerializeField, Range(0f, 1f)] private float hipOffset;
        [SerializeField, Range(float.Epsilon, .01f)] private float connectionWidth;
        [Header("Movement References")] 
        [SerializeField] private Material connectionMaterial;
        
        private GameObject movementParent;
        public MovementInformation left, right;

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
            left.SetTransform();
            right.SetTransform();
        }

        private void FindValidAnchors()
        {
            FindValidAnchor(left);
            FindValidAnchor(right);
        }

        private void CheckStates()
        {
            CheckState(left);
            CheckState(right);
        }

        private void FindValidAnchor(MovementInformation movementInformation)
        {
            RaycastHit[] allHits = Physics.SphereCastAll(movementInformation.Origin, castRadius, direction: movementInformation.CastVector, castDistance);
            IEnumerable<RaycastHit> validHits = allHits.Where(hit => hit.transform.TryGetComponent(out XRAttachableObject anchor));

            bool valid = false;
            float smallestAngle = float.PositiveInfinity;
            float smallestDistance = float.PositiveInfinity;
            XRAttachableObject attachableObject = null;
            RaycastHit validHit = new RaycastHit();

            foreach (RaycastHit hit in validHits)
            {
                hit.transform.TryGetComponent(out XRAttachableObject anchor);

                float angle = Vector3.Angle(movementInformation.CastVector, movementInformation.Origin - hit.point);
                float distance = Vector3.Distance(movementInformation.Origin, hit.point);
                
                if (!(angle <= smallestAngle)) continue;
                smallestAngle = angle;
                validHit = hit;
                attachableObject = anchor;
                valid = true;
            }
            
            movementInformation.SetAttachPoint(attachableObject, validHit, valid);
            movementInformation.AttachVisual();
        }

        private static void CheckState(MovementInformation movementInformation)
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
    }
}
