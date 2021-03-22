using System;
using DG.Tweening;
using Normal.Realtime;
using Project2.Scripts.Game_Logic;
using Project2.Scripts.Interfaces;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Movement;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Interaction
{
    public class XRInteractionInformation : MonoBehaviour
        {
            public XRInputController.Check handedness;

            /*private RaycastHit 
                validAnchorPoint, 
                attachedPoint;*/
            
            private Transform 
                castOrigin, 
                activeAnchor,
                potentialAnchor;
            
            private bool 
                validAnchorLocation, 
                attaching, 
                immediateDetach, 
                validGrabObject;
            
            private bool grabbed, gravity;
            private Transform grabbedObject, anchoredObject;
            private Rigidbody grabbedRigidbody;

            #region [Construction & References]
            
            private Vector3 ControllerPosition => XRInputController.Instance.Position(handedness);
            private Vector3 CastOriginPosition => castOrigin.position;
            private Vector3 CastDirection => castOrigin.forward;
            private Vector3 MagneticVector => (activeAnchor.transform.position - CastOriginPosition).normalized;
            private static Vector3 FinderDefaultPosition => new Vector3(0f, 0f, XRPlayerController.Instance.maximumInteractionDistance);
            public float ScaleFactor => Vector3.Distance(CastOriginPosition, potentialAnchor.position);
            private float LimitDistance => Vector3.Distance(CastOriginPosition, activeAnchor.position);

            #endregion

            public bool Anchored { get; private set; }
            public bool AnchoredAndMoving { get; private set; }
            
            /// <summary>
            /// Define what hand this is related to, and create construction geometry
            /// </summary>
            /// <param name="set"></param>
            public void InitialiseInteractionInformation(XRInputController.Check set)
            {
                SetHandedness(set);
                CreateConstructionGeometry();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="set"></param>
            private void SetHandedness(XRInputController.Check set)
            { 
                handedness = set;
                Debug.Log(message: $"{name} was set as {handedness.ToString()} handed!");
            }
            /// <summary>
            /// 
            /// </summary>
            private void CreateConstructionGeometry()
            {
                string handednessName = handedness.ToString();
                // The origin from which interactions are raycasted from
                castOrigin = Set.Object(gameObject, $"[Movement Origin] {handednessName}", position: Vector3.zero).transform;
                // The anchor that is linked with what you are currently interacting with
                activeAnchor = Set.Object(castOrigin.gameObject, $"[Magnet Anchor] {handednessName}", position: Vector3.zero).transform;
                // The anchor that will show you where you could interact with
                potentialAnchor = Set.Object(castOrigin.gameObject, $"[Finder Anchor] {handednessName}", position: Vector3.zero).transform;
            }


            private void Update()
            {
                // Find out what you are able to interact with
                CheckPotentialInteractions();
                // Then interact with it however makes sense
                CheckAnchorAttachmentState();
                // todo, a lot 🤔
                //magneticLasso.localPosition = LassoRestPosition;
                
                
                if (grabbed)
                {
                    grabbedRigidbody.AddForce((magneticLasso.position - grabbedObject.position) * playerController.magneticGrabForce);
                }
            }
            
            
            public void SetCastOriginTransform(float offset)
            {
                // Set the location of the hip positions, used for casting and locating anchor points
                castOrigin.localPosition = new Vector3(offset, 0f, -.05f);
                castOrigin.forward = Vector3.Lerp(castOrigin.forward, ControllerPosition - CastOriginPosition, playerController.finderDamping);
                
                // Calculate midpoints
                finderMidpoint.localPosition = new Vector3(0f, 0f, Mathf.Lerp(0f, potentialAnchor.localPosition.z, .5f));
                magnetMidpoint.localPosition = new Vector3(0f, 0f, Mathf.Lerp(0f, LimitDistance, .5f));
            }



            private void CheckPotentialInteractions()
            {
                // Debug information, used to visualise potential interaction state
                Color debug = Color.red;
                float castDistance = XRPlayerController.Instance.maximumInteractionDistance;
            
                // Raycast from the interaction origin
                if (Physics.Raycast(CastOriginPosition, CastDirection, out RaycastHit hit, castDistance))
                {
                    // todo, use some abstract base class quickly check and cache an object as interactive, rather than checking all of this every time
                    if (hit.transform.TryGetComponent(out InteractiveObjectInterfaces.ICanAttach canAttach))
                    {
                        ValidCurrentAnchorPoint(hit, true, false);
                        debug = Color.green;
                        castDistance = hit.distance;   
                    }
                    else if (hit.transform.TryGetComponent(out InteractiveObjectInterfaces.ICanGrab canGrab))
                    {
                        ValidCurrentAnchorPoint(hit, false, true);
                        debug = Color.yellow;
                        castDistance = hit.distance;
                    }
                }
                else
                {
                    NoValidCurrentAnchorPoint(); 
                }
            
                DrawVisuals();
                
                // Visualise potential interaction information
                Debug.DrawRay(CastOriginPosition, CastDirection.normalized * castDistance, debug);
            }

            private static XRInputController.XRControllerButton AttachAnchorTriggerButton() => XRPlayerController.Instance.moveTrigger;
            
            /// <summary>
            /// 
            /// </summary>
            private void CheckAnchorAttachmentState()
            {
                if (XRInputController.Instance.InputEvent(AttachAnchorTriggerButton()).State(handedness, XRInputController.InputEvents.InputEvent.Transition.Down))
                {
                    TriggerAttach();
                }
                else if (XRInputController.Instance.InputEvent(AttachAnchorTriggerButton()).State(handedness, XRInputController.InputEvents.InputEvent.Transition.Up))
                {
                    TriggerDetach(); 
                }
            }

            private void MovePlayer()
            {
                switch (XRPlayerController.Instance.PlayerMovementBehaviour)
                {
                    case XRPlayerController.MovementBehaviour.MoveWhenAnchored when Anchored:
                        MoveToAnchor();
                        // todo, put this when you anchor
                        if (DisableGravityOnForceApplied)
                        {
                            PlayerRigidbody.useGravity = !movementInformation.Attached;
                        }
                        break;
                    case XRPlayerController.MovementBehaviour.MoveWhenButtonPressed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (Anchored)
                {
                    
                }
                else
                {
                    if (DisableGravityOnForceApplied)
                    {
                        PlayerRigidbody.useGravity = useGravity;
                    }
                }
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
                potentialAnchor.position = Vector3.Lerp(potentialAnchor.position, validAnchorPoint.point, playerController.finderDamping);
                anchorVisual.forward = validAnchorPoint.normal;
                anchorVisual.gameObject.SetActive(true);
                validAnchorLocation = validAnchor;
                validGrabObject = validGrab;
                anchorFinder.material.SetColor(Colour, playerController.validAnchorColour);
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
                anchorFinder.material.SetColor(Colour, playerController.searchingAnchorColour);
            }
            /// <summary>
            /// Called when the user is not pointing directly at a valid point
            /// </summary>
            public void NoValidCurrentAnchorPoint()
            {
                // If the last valid anchor point is within a deviance from the origin, count it as valid
                
                if (Vector3.Angle(CastDirection, (validAnchorPoint.point - CastOriginPosition)) <= playerController.validPointDeviationLimit)
                {
                    //todo make this be smarter 
                    // needs to be checking if that location is "valid"
                    // not saying it is - in case objects are moving etc.
                    SearchForValidAnchorPoint(validAnchorPoint, validAnchorLocation, validGrabObject); 
                }
                else
                {
                    potentialAnchor.localPosition = Vector3.Lerp(potentialAnchor.localPosition, FinderDefaultPosition, playerController.finderDamping);
                    anchorVisual.gameObject.SetActive(false);
                    validAnchorLocation = false;
                    validGrabObject = false;
                    anchorFinder.material.SetColor(Colour, playerController.invalidAnchorColour);
                }
                return;
                potentialAnchor.localPosition = Vector3.Lerp(potentialAnchor.localPosition, FinderDefaultPosition, playerController.finderDamping);
                anchorVisual.gameObject.SetActive(false);
                validAnchorLocation = false;
                validGrabObject = false;
            }
            
            /// <summary>
            /// Draw the two curved lines
            /// </summary>
            public void DrawVisuals()
            {
                magnetVisual.BezierLine(CastOriginPosition, magnetMidpoint.position, activeAnchor.position);
                anchorFinder.BezierLine(CastOriginPosition, finderMidpoint.position, potentialAnchor.position);
            }

            public void TriggerAttach()
            {
                if (Anchored)
                {
                    Debug.Log($"{handedness}, tried to attach, but is already attached");
                    return;
                }
                
                if (validAnchorLocation)
                {
                    Debug.Log($"{handedness}, trying to attach to {validAnchorPoint.point}");
                    attaching = true;
                    attachedPoint = validAnchorPoint;
                    magnetVisual.material.SetColor(Colour, playerController.magnetAnchorColour);
                    magnetVisual.enabled = true;
                    activeAnchor.position = CastOriginPosition;
                    activeAnchor.DOMove(attachedPoint.point, playerController.attachDuration).OnComplete(AttachAnchor);
                }
                else if (validGrabObject)
                {
                    attaching = true;
                    attachedPoint = validAnchorPoint;
                    activeAnchor.position = CastOriginPosition;
                    magnetVisual.material.SetColor(Colour, playerController.magnetGrabColour);
                    magnetVisual.enabled = true;
                    activeAnchor.DOMove(attachedPoint.point, .1f).OnComplete(GrabObject);
                }
                else
                {
                    Debug.Log($"{handedness}, tried to attach, but there was no valid anchor location!");
                }
            }

            private void AttachAnchor()
            {
                attaching = false;
                Anchored = true;
                anchoredObject = attachedPoint.transform;
                activeAnchor.SetParent(anchoredObject);
                
                anchoredObject.GetComponent<InteractiveObjectInterfaces.ICanAttach>().Attach(playerController.magnetAnchorColour);
                
                AttachJoint();
                
                if (!immediateDetach) return;
                Debug.Log($"{handedness}, immediately detached");
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
                    GameController.Instance.CoupleTrigger = true;
                    if (!GameController.Instance.Ejected)
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

                grabbedObject.GetComponent<InteractiveObjectInterfaces.ICanGrab>().Grab(playerController.magnetGrabColour);
                
                if (grabbedObject.TryGetComponent(out RealtimeTransform realtimeTransform))
                {
                    realtimeTransform.RequestOwnership();
                }
                
                activeAnchor.SetParent(grabbedObject);
                gravity = grabbedRigidbody.useGravity;
                grabbedRigidbody.useGravity = false;
                grabbed = true;
                
                if (!immediateDetach) return;
                Debug.Log($"{handedness}, immediately detached");
                immediateDetach = false;
                TriggerDetach();
            }

            private void ReleaseObject()
            {
                Debug.Log($"Releasing {grabbedObject.name}!");
                activeAnchor.DOMove(CastOriginPosition, playerController.detachDuration).OnComplete(DetachAnchor);
                grabbed = false;
                grabbedRigidbody.velocity *= 2f;
                grabbedRigidbody.useGravity = gravity;
                grabbedObject.GetComponent<InteractiveObjectInterfaces.ICanGrab>().Release();
            }

            private void AttachJoint() { }

            public void TriggerDetach()
            {
                if (grabbed)
                {
                    ReleaseObject();
                    return;
                }
                
                if (!Anchored)
                {
                    if (attaching)
                    {
                        Debug.Log($"{handedness}, triggered detach while trying to attach, will immediately detach");
                        immediateDetach = true;
                    }
                    else
                    {
                        Debug.Log($"{handedness}, tried to detach anchor, but it is not attached to anything");
                    }
                }
                else
                {
                    Debug.Log($"{handedness}, detaching anchor");
                    anchoredObject.GetComponent<InteractiveObjectInterfaces.ICanAttach>().Detach();
                    activeAnchor.DOMove(CastOriginPosition, playerController.detachDuration).OnComplete(DetachAnchor);
                }
            }

            private void DetachAnchor()
            {
                magnetVisual.enabled = false;
                Anchored = false; 
                activeAnchor.SetParent(castOrigin);
            }
            
            /// <summary>
            /// Move the player 
            /// </summary>
            private void MoveToAnchor()
            {
                // todo define different behaviours for how to move, this is fine for now
                if (!Anchored) return;
                AnchoredAndMoving = true;
                Vector3 averageVector = ((MagneticVector + CastDirection) * 0.5f).normalized;
                Vector3 movementDirection = averageVector * XRPlayerController.Instance.movementForce;
                XRPlayerController.Instance.PlayerRigidbody.AddForce(movementDirection, ForceMode.Acceleration);
                Debug.DrawRay(CastOriginPosition, movementDirection, Color.red);
            }
        }
}