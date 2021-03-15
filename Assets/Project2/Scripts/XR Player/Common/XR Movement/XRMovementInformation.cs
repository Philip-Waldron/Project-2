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

            private RaycastHit validPoint, attachedPoint;
            private Transform origin, hit, midpoint;

            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 Origin => origin.position;
            public Vector3 CastVector => origin.forward;
            public Vector3 MoveVector => (attachedPoint.point - Origin).normalized;

            public bool Attached { get; set; }

            public void SetupMovementInformation(GameObject parent, XRInputController.Check set, float offset, Material material, float width)
            {
                check = set;
                origin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: Vector3.zero).transform;
                hit = Set.Object(null, $"[Movement Hit] {set.ToString()}", position: Vector3.zero).transform;
                midpoint = Set.Object(null, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
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
                if (Attached) return;
                hit.position = ControllerPosition;
            }

            public void AttachVisual()
            {
                connection.BezierLine(origin.position, midpoint.position, hit.position);
            }

            public void Attach()
            {
                Debug.Log("Attached");
                Attached = true;
                attachedPoint = validPoint;
                hit.position = attachedPoint.point;
            }

            public void Detach()
            {
                Debug.Log("Detached");
                Attached = false;
            }
        }
}