using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRInteractionInformation : XRInputAbstraction
        {
            public XRInputController.Check check;
            public LineRenderer magnetVisual, anchorFinder;
            private RaycastHit validAnchorPoint, attachedPoint;
            private Transform castOrigin, magnetAnchor, magnetMidpoint, finderAnchor, finderMidpoint, anchorVisual, magneticLasso;
            private ConfigurableJoint joint;
            private XRInteractionController interactionController;
            private bool validAnchorLocation, attaching, immediateDetach, validGrabObject;
            private float dynamicDistance;
            
            private bool grabbed, gravity;
            private Transform grabbedObject;
            private Outline grabbedOutline;
            private Rigidbody grabbedRigidbody;

            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 CastOriginPosition => castOrigin.position;
            public Vector3 CastVector => castOrigin.forward;
            private static Vector3 ManoeuvreVector => XRInputController.Forward(XRInputController.Check.Head);
            private Vector3 MagneticVector => (attachedPoint.point - CastOriginPosition).normalized;
            private Vector3 FinderDefaultPosition => new Vector3(0f, 0f, interactionController.maximumDistance);
            private float LimitDistance => Vector3.Distance(CastOriginPosition, magnetAnchor.position);
            
            private Vector3 LassoPosition => new Vector3(0f, 0f, interactionController.lassoOffset);

            private const float Bounciness = .1f;

            private InteractionState interactionState;
            private enum InteractionState
            {
                Move, Grab, Nothing
            }
            
            public bool Attached { get; private set; }

            private void LateUpdate()
            {
                magneticLasso.localPosition = LassoPosition;

                if (grabbed)
                {
                    grabbedRigidbody.AddForce((magneticLasso.position - grabbedObject.position) * interactionController.magneticGrabForce);
                    //grabbedObject.position = Vector3.Lerp(grabbedObject.position, magneticLasso.position, .2f);
                }
                
                /*
                if (Attached)
                {
                    dynamicDistance = LimitDistance;
                    joint.linearLimit = new SoftJointLimit()
                    {
                        limit = dynamicDistance,
                        bounciness = Bounciness
                    };
                    
                    return;
                    if (XRInputController.AxisDirection(check, XRInputController.Cardinal.Forward) || XRInputController.AxisDirection(check, XRInputController.Cardinal.Back))
                    {
                        dynamicDistance -= XRInputController.AxisValue(check).y * interactionController.reelingModifier;
                    }

                    joint.linearLimit = new SoftJointLimit()
                    {
                        limit = dynamicDistance,
                        bounciness = Bounciness
                    };
                }*/
            }

            public void SetupMovementInformation(XRInteractionController controller, GameObject parent, XRInputController.Check set, Material magnetMaterial, float magnetWidth, Material finderMaterial, float finderWidth)
            {
                interactionController = controller;
                    
                check = set;
                castOrigin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: Vector3.zero).transform;
                magnetAnchor = Set.Object(castOrigin.gameObject, $"[Magnet Anchor] {set.ToString()}", position: Vector3.zero).transform;
                magnetMidpoint = Set.Object(castOrigin.gameObject, $"[Movement Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                
                finderAnchor = Set.Object(castOrigin.gameObject, $"[Finder Anchor] {set.ToString()}", position: Vector3.zero).transform;
                finderMidpoint = Set.Object(castOrigin.gameObject, $"[Finder Midpoint] {set.ToString()}", position: Vector3.zero).transform;
                
                magneticLasso = Set.Object(castOrigin.gameObject, $"[Lasso] {set.ToString()}", position: LassoPosition).transform;

                magnetVisual = magnetAnchor.gameObject.Line(magnetMaterial, magnetWidth, startEnabled: false);
                anchorFinder = finderAnchor.gameObject.Line(finderMaterial, finderWidth);

                anchorVisual = Instantiate(interactionController.finderAnchorVisual).transform;
                anchorVisual.parent = finderAnchor;
                anchorVisual.ResetLocalTransform();
                
                ConfigureJoint();
            }

            private void ConfigureJoint()
            {
                joint = interactionController.gameObject.AddComponent<ConfigurableJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.enablePreprocessing = true;
                joint.enableCollision = false;
                joint.linearLimitSpring = new SoftJointLimitSpring()
                {
                    spring = 1500f,
                    damper = 250f
                };
                return;
                // Todo, make this not be dumb
                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;
            }

            public void SetTransform(float offset)
            {
                // Set the location of the hip positions, used for casting and locating anchor points
                castOrigin.localPosition = new Vector3(offset, 0f, -.025f);
                castOrigin.forward = Vector3.Lerp(castOrigin.forward, ControllerPosition - CastOriginPosition, interactionController.finderDamping);
                
                // Calculate midpoints
                finderMidpoint.localPosition = new Vector3(0f, 0f, Mathf.Lerp(0f, finderAnchor.localPosition.z, .5f));
                magnetMidpoint.localPosition = new Vector3(0f, 0f, Mathf.Lerp(0f, LimitDistance, .5f));
            }

            /// <summary>
            /// Called when the user is casting directly at a valid point
            /// </summary>
            /// <param name="raycastHit"></param>
            /// <param name="validAnchor"></param>
            /// <param name="validGrab"></param>
            public void ValidCurrentAnchorPoint(RaycastHit raycastHit, bool validAnchor, bool validGrab)
            {
                // Cache every valid anchor point;
                validAnchorPoint = raycastHit;
                finderAnchor.position = Vector3.Lerp(finderAnchor.position, validAnchorPoint.point, interactionController.finderDamping);
                anchorVisual.forward = validAnchorPoint.normal;
                anchorVisual.ScaleFactor(validAnchorPoint.distance);
                
                validAnchorLocation = validAnchor;
                validGrabObject = validGrab;
            }
            /// <summary>
            /// Called when the user is not pointing directly at a valid point
            /// </summary>
            public void NoValidCurrentAnchorPoint()
            {
                // If the last valid anchor point is within a deviance from the origin, count it as valid
                if (Vector3.Angle(CastVector, (validAnchorPoint.point - CastOriginPosition)) <= interactionController.devianceTolerance)
                {
                    ValidCurrentAnchorPoint(validAnchorPoint, validAnchorLocation, validGrabObject);
                }
                else
                {
                    finderAnchor.localPosition = Vector3.Lerp(finderAnchor.localPosition, FinderDefaultPosition, interactionController.finderDamping);
                    anchorVisual.ScaleFactor(0f);
                    validAnchorLocation = false;
                    validGrabObject = false;
                }
            }
            
            /// <summary>
            /// Draw the two curved lines
            /// </summary>
            public void DrawVisuals()
            {
                magnetVisual.BezierLine(CastOriginPosition, magnetMidpoint.position, magnetAnchor.position);
                anchorFinder.BezierLine(CastOriginPosition, finderMidpoint.position, finderAnchor.position);
            }

            public void TriggerAttach()
            {
                if (Attached)
                {
                    Debug.Log($"{check}, tried to attach, but is already attached");
                    return;
                }
                
                if (validAnchorLocation)
                {
                    Debug.Log($"{check}, trying to attach to {validAnchorPoint.point}");
                    attaching = true;
                    attachedPoint = validAnchorPoint;
                    magnetVisual.enabled = true;
                    magnetAnchor.position = CastOriginPosition;
                    magnetAnchor.DOMove(attachedPoint.point, interactionController.attachDuration).OnComplete(AttachAnchor);
                }
                else if (validGrabObject)
                {
                    attaching = true;
                    attachedPoint = validAnchorPoint;
                    magnetAnchor.position = CastOriginPosition;
                    magnetVisual.enabled = true;
                    magnetAnchor.DOMove(attachedPoint.point, .1f).OnComplete(GrabObject);
                }
                else
                {
                    Debug.Log($"{check}, tried to attach, but there was no valid anchor location!");
                }
            }

            private void AttachAnchor()
            {
                attaching = false;
                Attached = true;
                magnetAnchor.SetParent(attachedPoint.transform);
                AttachJoint();
                
                if (!immediateDetach) return;
                Debug.Log($"{check}, immediately detached");
                immediateDetach = false;
                TriggerDetach();
            }

            private void GrabObject()
            {
                attaching = false;
                grabbedObject = attachedPoint.transform;
                grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
                
                Debug.LogWarning($"Grabbing {grabbedObject.name}");

                if (grabbedRigidbody == null)
                {
                    Debug.LogWarning($"{grabbedObject.name} has no rigidbody attached, you goose!");
                    return;
                }

                grabbedRigidbody.velocity = Vector3.zero;
                magnetAnchor.SetParent(grabbedObject);
                gravity = grabbedRigidbody.useGravity;
                grabbedRigidbody.useGravity = false;
                grabbed = true;
                
                if (!immediateDetach) return;
                Debug.Log($"{check}, immediately detached");
                immediateDetach = false;
                TriggerDetach();
            }

            private void ReleaseObject()
            {
                Debug.LogWarning($"Releasing {grabbedObject.name}!");
                
                magnetAnchor.DOMove(CastOriginPosition, interactionController.detachDuration).OnComplete(DetachAnchor);
                grabbed = false;
                grabbedRigidbody.velocity *= 3f;
                //grabbedRigidbody.AddForce(CastVector * 15f, ForceMode.Impulse);
                grabbedRigidbody.useGravity = gravity;
            }

            private void AttachJoint()
            {
                return;
                joint.anchor = XRInputController.Transform().InverseTransformPoint(CastOriginPosition);
                joint.connectedAnchor = XRInputController.Transform().InverseTransformPoint(attachedPoint.point);
                
                joint.linearLimit = new SoftJointLimit()
                {
                    limit = LimitDistance,
                    bounciness = Bounciness
                };

                dynamicDistance = LimitDistance;
                
                SetJointFreedom(ConfigurableJointMotion.Limited);
            }

            public void TriggerDetach()
            {
                if (grabbed)
                {
                    ReleaseObject();
                    return;
                }
                
                if (!Attached)
                {
                    if (attaching)
                    {
                        Debug.Log($"{check}, triggered detach while trying to attach, will immediately detach");
                        immediateDetach = true;
                    }
                    else
                    {
                        Debug.Log($"{check}, tried to detach anchor, but it is not attached to anything");
                    }
                }
                else
                {
                    Debug.Log($"{check}, detaching anchor");
                    magnetAnchor.DOMove(CastOriginPosition, interactionController.detachDuration).OnComplete(DetachAnchor);
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
                SetJointFreedom(ConfigurableJointMotion.Free);
            }

            private void SetJointFreedom(ConfigurableJointMotion state)
            {
                return;
                joint.xMotion = state;
                joint.yMotion = state;
                joint.zMotion = state;
            }

            public void MoveToAnchor()
            {
                if (!Attached) return;
                
                /*
                dynamicDistance -= .75f;
                
                joint.linearLimit = new SoftJointLimit()
                {
                    limit = dynamicDistance,
                    bounciness = Bounciness
                };

                return;
                */
                
                interactionController.PlayerRigidbody.AddForce(MagneticVector * interactionController.magneticForce, ForceMode.Acceleration);
                interactionController.PlayerRigidbody.AddForce(ManoeuvreVector * interactionController.manoeuvreForce, ForceMode.Acceleration);
                interactionController.PlayerRigidbody.AddForce(CastVector * interactionController.castForce, ForceMode.Acceleration);
                
                Debug.DrawRay(CastOriginPosition, MagneticVector * interactionController.magneticForce, Color.blue);
                Debug.DrawRay(CastOriginPosition, ManoeuvreVector * interactionController.manoeuvreForce, Color.cyan);
                Debug.DrawRay(CastOriginPosition, CastVector * interactionController.castForce, Color.green);
            }
        }
}