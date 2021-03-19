using System;
using DG.Tweening;
using Project2.Scripts.Game_Logic;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;
using Outline = VR_Prototyping.Plugins.QuickOutline.Scripts.Outline;

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
            
            private bool grabbed, gravity;
            private Transform grabbedObject, anchoredObject;
            private Rigidbody grabbedRigidbody;
            private static readonly int State = Shader.PropertyToID("_State");
            private static readonly int Colour = Shader.PropertyToID("_Colour");

            private Vector3 ControllerPosition => XRInputController.Position(check);
            public Vector3 CastOriginPosition => castOrigin.position;
            public Vector3 CastVector => castOrigin.forward;
            private static Vector3 ManoeuvreVector => XRInputController.Forward(XRInputController.Check.Head);
            private Vector3 MagneticVector => (attachedPoint.point - CastOriginPosition).normalized;
            private Vector3 FinderDefaultPosition => new Vector3(0f, 0f, interactionController.maximumDistance);
            private float ScaleFactor => Vector3.Distance(CastOriginPosition, finderAnchor.position);
            private float LimitDistance => Vector3.Distance(CastOriginPosition, magnetAnchor.position);
            
            private Vector3 LassoPosition => new Vector3(0f, 0f, interactionController.lassoOffset);

            public bool Attached { get; private set; }

            private void Update()
            {
                magneticLasso.localPosition = LassoPosition;
                anchorVisual.ScaleFactor(ScaleFactor);
                
                if (grabbed)
                {
                    grabbedRigidbody.AddForce((magneticLasso.position - grabbedObject.position) * interactionController.magneticGrabForce); 
                }
            }

            public void SetupMovementInformation(XRInteractionController controller, GameObject parent, XRInputController.Check set, Material magnetMaterial, float magnetWidth, Material finderMaterial, float finderWidth)
            {
                interactionController = controller;
                    
                check = set;
                castOrigin = Set.Object(parent, $"[Movement Origin] {set.ToString()}", position: Vector3.zero).transform;
                GameObject visual = Instantiate(interactionController.castOrigin, castOrigin, worldPositionStays: true);

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

            private void ConfigureJoint() { }

            public void SetTransform(float offset)
            {
                // Set the location of the hip positions, used for casting and locating anchor points
                castOrigin.localPosition = new Vector3(offset, 0f, -.05f);
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
                anchorVisual.gameObject.SetActive(true);
                validAnchorLocation = validAnchor;
                validGrabObject = validGrab;
                anchorFinder.material.SetColor(Colour, interactionController.validAnchorColour);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="raycastHit"></param>
            /// <param name="validAnchor"></param>
            /// <param name="validGrab"></param>
            private void SearchForValidAnchorPoint(RaycastHit raycastHit, bool validAnchor, bool validGrab)
            {
                ValidCurrentAnchorPoint(raycastHit, validAnchor, validGrab);
                anchorFinder.material.SetColor(Colour, interactionController.searchingAnchorColour);
            }
            /// <summary>
            /// Called when the user is not pointing directly at a valid point
            /// </summary>
            public void NoValidCurrentAnchorPoint()
            {
                // If the last valid anchor point is within a deviance from the origin, count it as valid
                
                if (Vector3.Angle(CastVector, (validAnchorPoint.point - CastOriginPosition)) <= interactionController.devianceTolerance)
                {
                    //todo make this be smarter 
                    // needs to be checking if that location is "valid"
                    // not saying it is - in case objects are moving etc.
                    SearchForValidAnchorPoint(validAnchorPoint, validAnchorLocation, validGrabObject); 
                }
                else
                {
                    finderAnchor.localPosition = Vector3.Lerp(finderAnchor.localPosition, FinderDefaultPosition, interactionController.finderDamping);
                    anchorVisual.gameObject.SetActive(false);
                    validAnchorLocation = false;
                    validGrabObject = false;
                    anchorFinder.material.SetColor(Colour, interactionController.invalidAnchorColour);
                }
                return;
                finderAnchor.localPosition = Vector3.Lerp(finderAnchor.localPosition, FinderDefaultPosition, interactionController.finderDamping);
                anchorVisual.gameObject.SetActive(false);
                validAnchorLocation = false;
                validGrabObject = false;
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
                    magnetVisual.material.SetColor(Colour, interactionController.magnetAnchorColour);
                    magnetVisual.enabled = true;
                    magnetAnchor.position = CastOriginPosition;
                    magnetAnchor.DOMove(attachedPoint.point, interactionController.attachDuration).OnComplete(AttachAnchor);
                }
                else if (validGrabObject)
                {
                    attaching = true;
                    attachedPoint = validAnchorPoint;
                    magnetAnchor.position = CastOriginPosition;
                    magnetVisual.material.SetColor(Colour, interactionController.magnetGrabColour);
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
                anchoredObject = attachedPoint.transform;
                magnetAnchor.SetParent(anchoredObject);
                
                if (anchoredObject.TryGetComponent(out Outline outline))
                {
                    outline.OutlineColor = interactionController.magnetAnchorColour;
                    outline.enabled = true;
                }
                else
                {
                    Outline newOutline = anchoredObject.gameObject.AddComponent<Outline>();
                    newOutline.OutlineColor = interactionController.magnetAnchorColour;
                    newOutline.enabled = true;
                }
                
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
                
                if (grabbedObject.TryGetComponent(out Bomb bomb))
                {
                    interactionController.GameController.CoupleTrigger = true;
                    if (!interactionController.GameController.Ejected)
                    {
                        immediateDetach = true;
                        Debug.Log($"<b>Grabbing the bomb for the first time, will couple with player!</b>");
                        return;
                    }
                    Debug.Log($"<b>Grabbing the bomb!</b>");
                }
                else
                {
                    Debug.Log($"Grabbing regular ol' object: <b>{grabbedObject.name}</b>");
                }

                if (grabbedRigidbody == null)
                {
                    Debug.LogWarning($"{grabbedObject.name} has no rigidbody attached, you goose!");
                    return;
                }

                // grabbedRigidbody.velocity = Vector3.zero;

                if (grabbedObject.TryGetComponent(out VR_Prototyping.Plugins.QuickOutline.Scripts.Outline outline))
                {
                    outline.OutlineColor = interactionController.magnetGrabColour;
                    outline.enabled = true;
                }
                
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
                Debug.Log($"Releasing {grabbedObject.name}!");
                magnetAnchor.DOMove(CastOriginPosition, interactionController.detachDuration).OnComplete(DetachAnchor);
                grabbed = false;
                grabbedRigidbody.velocity *= 2f;
                grabbedRigidbody.useGravity = gravity;
                if (grabbedObject.TryGetComponent(out VR_Prototyping.Plugins.QuickOutline.Scripts.Outline outline))
                {
                    outline.enabled = false;
                }
            }

            private void AttachJoint() { }

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
                    if (anchoredObject.TryGetComponent(out VR_Prototyping.Plugins.QuickOutline.Scripts.Outline outline))
                    {
                        outline.enabled = false;
                    }
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

            private void SetJointFreedom(ConfigurableJointMotion state) { }

            public void MoveToAnchor()
            {
                if (!Attached) return;
                
                // Debug.Log("Adding force!!");

                if (interactionController.UseMagneticForce)
                {
                    interactionController.PlayerRigidbody.AddForce(MagneticVector * interactionController.MagneticForce, ForceMode.Acceleration);
                    Debug.DrawRay(CastOriginPosition, MagneticVector * interactionController.MagneticForce, Color.blue);
                }

                if (interactionController.UseCastForce)
                {
                    interactionController.PlayerRigidbody.AddForce(CastVector * interactionController.CastForce, ForceMode.Acceleration);
                    Debug.DrawRay(CastOriginPosition, CastVector * interactionController.CastForce, Color.green);
                }

                if (interactionController.UseManoeuvreForce)
                {
                    interactionController.PlayerRigidbody.AddForce(ManoeuvreVector * interactionController.ManoeuvreForce, ForceMode.Acceleration);
                    Debug.DrawRay(CastOriginPosition, ManoeuvreVector * interactionController.ManoeuvreForce, Color.cyan);
                }

                if (interactionController.UseAverageForce)
                {
                    Vector3 averageVector = ((MagneticVector + CastVector) * 0.5f).normalized;
                    interactionController.PlayerRigidbody.AddForce(averageVector * interactionController.AverageForce, ForceMode.Acceleration);
                    Debug.DrawRay(CastOriginPosition, averageVector * interactionController.AverageForce, Color.yellow);
                }
            }
        }
}