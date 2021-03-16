using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRMovementInformation : XRInputAbstraction
        {
            public XRInputController.Check check;
            public LineRenderer magnetVisual, anchorFinder;
            private RaycastHit validAnchorPoint, attachedPoint;
            private Transform castOrigin, magnetAnchor, magnetMidpoint, finderAnchor, finderMidpoint, anchorVisual;
            private ConfigurableJoint joint;
            private XRMovementController movementController;
            private bool validAnchorLocation, immediateDetach;
            
            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 CastOriginPosition => castOrigin.position;
            public Vector3 CastVector => castOrigin.forward;
            public Vector3 MoveVector => (attachedPoint.point - CastOriginPosition).normalized;
            public Vector3 FinderDefaultPosition => new Vector3(0f, 0f, movementController.maximumDistance);
            private float Distance => Vector3.Distance(CastOriginPosition, magnetAnchor.position);

            public bool Attached { get; set; }

            public void SetupMovementInformation(XRMovementController controller, GameObject parent, XRInputController.Check set, Material magnetMaterial, float magnetWidth, Material finderMaterial, float finderWidth)
            {
                movementController = controller;
                    
                check = set;
                castOrigin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: Vector3.zero).transform;
                
                magnetAnchor = Set.Object(castOrigin.gameObject, $"[Magnet Anchor] {set.ToString()}", position: Vector3.zero).transform;
                magnetMidpoint = Set.Object(null, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                
                finderAnchor = Set.Object(castOrigin.gameObject, $"[Finder Anchor] {set.ToString()}", position: Vector3.zero).transform;
                finderMidpoint = Set.Object(castOrigin.gameObject, $"[Finder Midpoint] {set.ToString()}", position: Vector3.zero).transform;

                magnetVisual = magnetAnchor.gameObject.Line(magnetMaterial, magnetWidth, startEnabled: false);
                anchorFinder = finderAnchor.gameObject.Line(finderMaterial, finderWidth);

                anchorVisual = Instantiate(movementController.finderAnchorVisual).transform;
                anchorVisual.parent = finderAnchor;
                anchorVisual.ResetLocalTransform();
                
                joint = movementController.gameObject.AddComponent<ConfigurableJoint>();
            }

            private void Update()
            {
                magnetVisual.BezierLine(castOrigin.position, magnetMidpoint.position, magnetAnchor.position);
            }

            public void SetTransform(float hipOffset, float heightOffset)
            {
                // Set the location of the hip positions, used for casting and locating anchor points
                castOrigin.LookAt(ControllerPosition);
                castOrigin.localPosition = new Vector3(hipOffset, 0f, -.01f);
                Vector3 position = castOrigin.position;
                position = new Vector3(position.x, XRInputController.Position(XRInputController.Check.Head).y - heightOffset, position.z);
                castOrigin.position = position;
                // Calculate midpoints
                finderMidpoint.localPosition = new Vector3(0f, 0f, Mathf.Lerp(0f, finderAnchor.localPosition.z, .5f));
                magnetMidpoint.LerpMidpoint(castOrigin, magnetAnchor, movementController.magnetDamping);
            }
            /// <summary>
            /// Called when the user is casting directly at a valid point
            /// </summary>
            /// <param name="raycastHit"></param>
            public void ValidCurrentAnchorPoint(RaycastHit raycastHit)
            {
                // Cache every valid anchor point
                validAnchorPoint = raycastHit;
                finderAnchor.position = Vector3.Lerp(finderAnchor.position, validAnchorPoint.point, movementController.finderDamping);
                anchorVisual.forward = validAnchorPoint.normal;
                anchorVisual.ScaleFactor(validAnchorPoint.distance);
                validAnchorLocation = true;
            }
            /// <summary>
            /// Called when the user is not pointing directly at a valid point
            /// </summary>
            public void NoValidCurrentAnchorPoint()
            {
                // If the last valid anchor point is within a deviance from the origin, count it as valid
                if (Vector3.Angle(CastVector, (validAnchorPoint.point - CastOriginPosition)) <= movementController.devianceTolerance)
                {
                    ValidCurrentAnchorPoint(validAnchorPoint);
                }
                else
                {
                    finderAnchor.localPosition = Vector3.Lerp(finderAnchor.localPosition, FinderDefaultPosition, movementController.finderDamping);
                    anchorVisual.ScaleFactor(0f);
                    validAnchorLocation = false;
                }
            }
            
            /// <summary>
            /// Draw the finder curve
            /// </summary>
            public void FinderVisual()
            {
                anchorFinder.BezierLine(castOrigin.position, finderMidpoint.position, finderAnchor.position);
            }

            public void TriggerAttach()
            {
                if (Attached)
                {
                    Debug.Log($"{check}, tried to attach, but is already attached");
                    return;
                }
                if (!validAnchorLocation)
                {
                    Debug.Log($"{check}, tried to attach, but there was no valid anchor location!");
                }
                else
                {
                    Debug.Log($"{check}, trying to attach to {validAnchorPoint.point}");
                    attachedPoint = validAnchorPoint;
                    magnetVisual.enabled = true;
                    magnetAnchor.DOMove(attachedPoint.point, movementController.attachDuration).OnComplete(AttachAnchor);
                }
            }

            private void AttachAnchor()
            {
                Attached = true;
                magnetAnchor.position = attachedPoint.point;
                magnetAnchor.SetParent(attachedPoint.transform);
                AttachJoint();
                
                if (!immediateDetach) return;
                Debug.Log($"{check}, immediately detached");
                immediateDetach = false;
                DetachAnchor();
            }
            
            private void AttachJoint()
            {
                joint.connectedBody = attachedPoint.rigidbody;
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = attachedPoint.transform.InverseTransformPoint(magnetAnchor.position);
                
                joint.linearLimit = new SoftJointLimit()
                {
                    limit = Distance,
                    bounciness = 0.3f
                };
                
                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;
            }

            public void TriggerDetach()
            {
                if (!Attached)
                {
                    immediateDetach = true;
                    Debug.Log($"{check}, tried to detach anchor, but it is not attached to anything");
                }
                else
                {
                    Debug.Log($"{check}, detaching anchor");
                    magnetAnchor.DOMove(CastOriginPosition, movementController.detachDuration).OnComplete(DetachAnchor);
                }
            }

            private void DetachAnchor()
            {
                magnetVisual.enabled = false;
                Attached = false;
                DetachJoint();
                magnetAnchor.SetParent(castOrigin);
            }

            private void DetachJoint()
            {
                joint.xMotion = ConfigurableJointMotion.Free;
                joint.yMotion = ConfigurableJointMotion.Free;
                joint.zMotion = ConfigurableJointMotion.Free;
                //Destroy(ConfigurableJoint);
            }
        }
}