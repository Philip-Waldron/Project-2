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
            private RaycastHit validPoint, attachedPoint;
            private Transform origin, hit, midpoint;
            private bool valid;

            private ConfigurableJoint ConfigurableJoint;
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
                hit = Set.Object(null, $"[Movement Hit] {set.ToString()}", position: Vector3.zero).transform;
                midpoint = Set.Object(null, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                connection = origin.gameObject.Line(material, width);
                playerRigidbody = player;
                ConfigurableJoint = player.gameObject.AddComponent<ConfigurableJoint>();
            }

            private void Update()
            {
                return;
                
                if (!Attached) return;
                
                ConfigurableJoint.anchor = attachedPoint.transform.InverseTransformPoint(hit.position);
                ConfigurableJoint.linearLimit = new SoftJointLimit()
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
                midpoint.LerpMidpoint(origin, hit, .3f);
            }

            public void SetAttachPoint(RaycastHit raycastHit)
            {
                validPoint = raycastHit;
                if (Attached) return;
                valid = true;
                hit.position = Vector3.Lerp(hit.position, raycastHit.point, .5f);
            }

            public void ClearAttachPoint()
            {
                if (Attached) return;
                valid = false;
                hit.position = ControllerPosition;
            }

            public void AttachVisual()
            {
                connection.BezierLine(origin.position, midpoint.position, hit.position);
            }

            public void Attach()
            {
                if (!valid) return;
                
                Attached = true;
                attachedPoint = validPoint;
                hit.position = attachedPoint.point;
                hit.SetParent(attachedPoint.transform);
                
                ConfigurableJoint.connectedBody = attachedPoint.rigidbody;
                ConfigurableJoint.connectedAnchor = attachedPoint.transform.InverseTransformPoint(hit.position);
                
                ConfigurableJoint.linearLimit = new SoftJointLimit()
                {
                    limit = Distance,
                    bounciness = 0.3f
                };
                
                ConfigurableJoint.xMotion = ConfigurableJointMotion.Limited;
                ConfigurableJoint.yMotion = ConfigurableJointMotion.Limited;
                ConfigurableJoint.zMotion = ConfigurableJointMotion.Limited;
            }

            public void Detach()
            {
                Attached = false;
                hit.SetParent(null);
                
                ConfigurableJoint.xMotion = ConfigurableJointMotion.Free;
                ConfigurableJoint.yMotion = ConfigurableJointMotion.Free;
                ConfigurableJoint.zMotion = ConfigurableJointMotion.Free;
                //Destroy(ConfigurableJoint);
            }
        }
}