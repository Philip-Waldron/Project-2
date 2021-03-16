using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRMovementInformation : XRInputAbstraction
        {
            public XRInputController.Check check;
            public LineRenderer connection;
            private Rigidbody playerRigidbody;
            private RaycastHit validPoint, lastValid, attachedPoint;
            private Transform origin, hit, midpoint;
            private bool valid;

            private float validWidth;
            private float InvalidWidth => validWidth * .25f;

            private ConfigurableJoint joint;
            // private ConfigurableJoint ConfigurableJoint => origin.GetComponent<ConfigurableJoint>();
            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 Origin => origin.position;
            public Vector3 CastVector => origin.forward;
            public Vector3 MoveVector => (attachedPoint.point - Origin).normalized;
            private float Distance => Vector3.Distance(Origin, hit.position);

            public bool Attached { get; set; }

            public void SetupMovementInformation(GameObject parent, XRInputController.Check set, Material material, float width, Rigidbody player)
            {
                check = set;
                origin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: Vector3.zero).transform;
                hit = Set.Object(origin.gameObject, $"[Movement Hit] {set.ToString()}", position: new Vector3(0f, 0f, 3f)).transform;
                midpoint = Set.Object(null, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                validWidth = width;
                connection = origin.gameObject.Line(material, InvalidWidth);
                playerRigidbody = player;
                joint = player.gameObject.AddComponent<ConfigurableJoint>();
            }

            private void Update()
            {
                return;
                
                if (!Attached) return;
                
                joint.anchor = attachedPoint.transform.InverseTransformPoint(hit.position);
                joint.linearLimit = new SoftJointLimit()
                {
                    limit = Distance,
                    bounciness = 0.3f
                };
            }

            public void SetTransform(float hipOffset, float heightOffset)
            {
                origin.LookAt(ControllerPosition);
                origin.localPosition = new Vector3(hipOffset, 0f, -.01f);
                Vector3 position = origin.position;
                position = new Vector3(position.x, XRInputController.Position(XRInputController.Check.Head).y - heightOffset, position.z);
                origin.position = position;
                midpoint.LerpMidpoint(origin, hit, 1f);
            }

            public void SetAttachPoint(RaycastHit raycastHit)
            {
                validPoint = raycastHit;
                lastValid = validPoint;
                if (Attached) return;
                valid = true;
                hit.position = Vector3.Lerp(hit.position, raycastHit.point, 1f);
                connection.Width(validWidth);
            }

            public void ClearAttachPoint()
            {
                if (Attached) return;
                valid = false;
                
                hit.localPosition = new Vector3(0f, 0f, 3f);
                connection.Width(InvalidWidth);
            }

            public void AttachVisual()
            {
                connection.BezierLine(origin.position, midpoint.position, hit.position);
            }

            public void Attach()
            {
                // if (!valid) return;
                
                Attached = true;
                attachedPoint = lastValid;
                hit.position = attachedPoint.point;
                hit.SetParent(attachedPoint.transform);
                
                joint.connectedBody = attachedPoint.rigidbody;
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = attachedPoint.transform.InverseTransformPoint(hit.position);
                
                joint.linearLimit = new SoftJointLimit()
                {
                    limit = Distance,
                    bounciness = 0.3f
                };
                
                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;
            }

            public void Detach()
            {
                Attached = false;
                hit.SetParent(origin);
                
                joint.xMotion = ConfigurableJointMotion.Free;
                joint.yMotion = ConfigurableJointMotion.Free;
                joint.zMotion = ConfigurableJointMotion.Free;
                //Destroy(ConfigurableJoint);
            }
        }
}