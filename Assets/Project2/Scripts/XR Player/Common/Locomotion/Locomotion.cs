using System;
using System.Collections.Generic;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;
using Transition = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.InputEvents.InputEvent.Transition;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;
using Event = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Event;
using GestureType = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.InputGestures.InputGesture.GestureType;

namespace XR_Prototyping.Scripts.Common.Locomotion
{
    public class Locomotion : XRInputAbstraction
    {
        public LocomotionType locomotionType;
        
        private LocomotionVisual locomotionVisual;
        private LocomotionGhost locomotionGhost;
        private LocomotionTarget volumeTarget;
        
        private Transform controllerProxy, locomotionParent, locomotionTarget, visualTransform, ghostTransform, stepBack;
        private LineRenderer locomotionArc;

        private Check locomotionHand = Check.Head, gestureHand = Check.Head, externalTriggerHand;

        private float dynamicArcWidth;
        private bool casting, cooldown, teleporting, gesture, valid, externalTrigger;

        private Vector3 visualForward, visualRotation;
        private const float OffsetAngle = 0f, OffsetFromGround = 0.005f;
        private readonly List<Vector3> arcPoints = new List<Vector3>();

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            locomotionParent = Set.Object(null, "[Locomotion Parent]", Vector3.zero).transform;
            stepBack = Set.Object(locomotionParent.gameObject, "[Locomotion Parent]", Vector3.zero).transform;
            controllerProxy = Set.Object(gameObject, "[Controller Proxy]", Vector3.zero).transform;
            locomotionTarget = Set.Object(gameObject, "[Locomotion Target]", Vector3.zero).transform;
            
            visualTransform = Instantiate(locomotionType.locomotionVisualPrefab, locomotionTarget).transform;
            locomotionVisual = visualTransform.GetComponent<LocomotionVisual>();
            locomotionVisual.SetupVisual(cachedLocomotion: this);

            ghostTransform = Instantiate(locomotionType.locomotionGhostPrefab, locomotionVisual.GhostParent()).transform;
            locomotionGhost = ghostTransform.GetComponent<LocomotionGhost>();
            locomotionGhost.DisableGhost();
            
            locomotionArc = gameObject.Line(
                material: locomotionType.arcMaterial, 
                startWidth: locomotionType.arcWidth,
                endWidth: locomotionType.arcWidth, 
                startEnabled: false, 
                worldSpace: true,
                positions: locomotionType.arcResolution);
        }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            Cast();
            Gesture();
        }
        /// <summary>
        /// 
        /// </summary>
        private void Cast()
        {
            if (Valid() && LocomotionStart(out Check check))
            {
                switch (locomotionType.locomotionHandedness)
                {
                    case LocomotionType.LocomotionHandedness.DominantOnly when check == XRInputController.DominantHand():
                        locomotionHand = check;
                        CastStart();
                        break;
                    case LocomotionType.LocomotionHandedness.NonDominantOnly when check == XRInputController.NonDominantHand():
                        locomotionHand = check;
                        CastStart();
                        break;
                    case LocomotionType.LocomotionHandedness.Both:
                        locomotionHand = check;
                        CastStart();
                        break;
                    default:
                        break;
                }
            }
            if (casting)
            {
                locomotionArc.Width(dynamicArcWidth, dynamicArcWidth);
                
                if (LocomotionSwap(check: XRInputController.OtherHand(locomotionHand)))
                {
                    XRInteractionController.SetAllowedState(locomotionHand, state: true);
                    locomotionHand = XRInputController.OtherHand(locomotionHand);
                    XRInteractionController.SetAllowedState(locomotionHand, state: false);
                }
                
                CastStay();
                
                if (LocomotionEnd(locomotionHand))
                {
                    CastEnd();
                    Teleport();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool LocomotionStart(out Check check)
        {
            bool validStart;
            check = Check.Head;
            
            switch (locomotionType.locomotionStartTrigger)
            {
                case LocomotionType.LocomotionStartTrigger.JoystickMovement:
                    validStart =  XRInputController.AxisDirection(XRInputController.Cardinal.Forward, out Check joystickForward);
                    check = joystickForward;
                    break;
                case LocomotionType.LocomotionStartTrigger.TouchstripTouch:
                    validStart = XRInputController.InputEvent(Event.AnalogTouch).State(Transition.Down, out Check touchpadTouch);
                    check = touchpadTouch;
                    break;
                case LocomotionType.LocomotionStartTrigger.ExternalTrigger:
                    check = externalTriggerHand;
                    return externalTrigger;
                case LocomotionType.LocomotionStartTrigger.TouchstripLongPress:
                    validStart = XRInputController.InputGesture(XRInputController.Gesture.AnalogGesture).State(GestureType.LongPress, out Check longPress);
                    check = longPress;
                    break;
                case LocomotionType.LocomotionStartTrigger.TouchstripSingleTap:
                    validStart = XRInputController.InputGesture(XRInputController.Gesture.AnalogGesture).State(GestureType.SingleTap, out Check singleTap);
                    check = singleTap;
                    break;
                default:
                    return false;
            }

            return validStart && XRInteractionController.QueryInteractionAvailability(check);
        }    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool LocomotionSwap(Check check)
        {
            switch (locomotionType.locomotionStartTrigger)
            {
                case LocomotionType.LocomotionStartTrigger.JoystickMovement:
                    return XRInputController.AxisDirection(check, XRInputController.Cardinal.Forward);
                case LocomotionType.LocomotionStartTrigger.TouchstripTouch:
                    return XRInputController.InputEvent(Event.AnalogTouch).State(check, Transition.Down);
                case LocomotionType.LocomotionStartTrigger.ExternalTrigger:
                    // Todo, this.
                    return false;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool LocomotionEnd(Check check)
        {
            switch (locomotionType.locomotionEndTrigger)
            {
                case LocomotionType.LocomotionEndTrigger.TouchstripRelease:
                    return XRInputController.InputEvent(Event.AnalogTouch).State(check, Transition.Up);
                case LocomotionType.LocomotionEndTrigger.TouchstripTouch:
                    return XRInputController.InputEvent(Event.AnalogTouch).State(check, Transition.Down);
                case LocomotionType.LocomotionEndTrigger.ExternalTrigger:
                    return !externalTrigger;
                case LocomotionType.LocomotionEndTrigger.JoystickRecenter:
                    return XRInputController.AxisDirection(check, XRInputController.Cardinal.Center);
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Gesture()
        {
            if (locomotionType.locomotionStartTrigger != LocomotionType.LocomotionStartTrigger.JoystickMovement) return;
            stepBack.transform.localPosition = new Vector3(0,0, - locomotionType.stepBackDistance);
            
            if (XRInputController.AxisDirection(gestureHand, XRInputController.Cardinal.Center))
            {
                gesture = false;
            }

            if (Valid())
            {
                if (XRInputController.AxisDirection(XRInputController.Cardinal.Left, out Check gestureLeft))
                {
                    SnapRotate(-locomotionType.snapRotationAngle);
                    gestureHand = gestureLeft;
                    return;
                }
                if (XRInputController.AxisDirection(XRInputController.Cardinal.Right, out Check gestureRight))
                {
                    SnapRotate(locomotionType.snapRotationAngle);
                    gestureHand = gestureRight;
                    return;
                }
                if (XRInputController.AxisDirection(XRInputController.Cardinal.Back, out Check gestureBack))
                {
                    gestureHand = gestureBack;
                    switch (locomotionType.backwardsGestureEffect)
                    {
                        case LocomotionType.BackwardsGestureEffect.StepBack:
                            StepBack();
                            return;
                        case LocomotionType.BackwardsGestureEffect.TurnAround:
                            SnapRotate(180f);
                            return;
                        default:
                            return;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Teleport()
        {
            if (!Valid() || !valid) return;
            Couple();
            
            teleporting = true;
            locomotionVisual.TeleportInformation(out Vector3 position, out Vector3 rotation);
            
            switch (locomotionType.locomotionVisualEffect)
            {
                case LocomotionType.LocomotionVisualEffect.Dash:
                    locomotionVisual.SetTargetState(true);
                    locomotionParent.DOMove(position, locomotionType.dashDuration).OnComplete(Decouple);
                    locomotionParent.DORotate(rotation, locomotionType.dashDuration);
                    break;
                case LocomotionType.LocomotionVisualEffect.Blink:
                    locomotionParent.position = position;
                    locomotionParent.eulerAngles = rotation;
                    Decouple();
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SnapRotate(float angle)
        {
            if (!Valid()) return;
            Couple();
            gesture = true;
            
            float 
                currentAngle = locomotionParent.eulerAngles.y, 
                targetAngle = currentAngle + angle;
            Vector3 targetRotation = new Vector3(0, targetAngle, 0);

            switch (locomotionType.locomotionVisualEffect)
            {
                case LocomotionType.LocomotionVisualEffect.Dash:
                    locomotionParent.DORotate(targetRotation, locomotionType.gestureDuration).OnComplete(Decouple);
                    break;
                case LocomotionType.LocomotionVisualEffect.Blink:
                    locomotionParent.eulerAngles = targetRotation;
                    Decouple();
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void StepBack()
        {
            if (!Valid()) return;
            
            Couple();
            gesture = true;
            stepBack.SetParent(null);
            
            switch (locomotionType.locomotionVisualEffect)
            {
                case LocomotionType.LocomotionVisualEffect.Dash:
                    locomotionParent.DOMove(stepBack.position, locomotionType.gestureDuration).OnComplete(Decouple);
                    break;
                case LocomotionType.LocomotionVisualEffect.Blink:
                    locomotionParent.position = stepBack.position;
                    Decouple();
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Couple()
        {
            XRVisualEffectsController.SetVignetteStrength(locomotionType.vignetteStrength);
            locomotionParent.position = XRInputController.NormalisedPosition(Check.Head);
            locomotionParent.eulerAngles = XRInputController.NormalisedRotation(Check.Head);
            XRInputController.Transform().SetParent(locomotionParent);
        }
        /// <summary>
        /// 
        /// </summary>
        private void Decouple()
        {
            XRVisualEffectsController.SetVignetteStrength(0f);
            locomotionVisual.SetTargetState(false);
            XRInputController.Transform().SetParent(null);
            stepBack.SetParent(locomotionParent);
            teleporting = false;
            if (volumeTarget != null)
            {
                volumeTarget.locomotionTrigger.Invoke();
            }
            volumeTarget = null;
        }
        /// <summary>
        /// 
        /// </summary>
        private void CastStart()
        {
            XRInteractionController.SetAllowedState(locomotionHand, state: false);
            casting = true;
            locomotionArc.enabled = true;
            locomotionVisual.EnableVisual();
            locomotionGhost.EnableGhost();
            controllerProxy.transform.Transforms(XRInputController.Transform(locomotionHand));
            DOTween.To(() => dynamicArcWidth, x => dynamicArcWidth = x, locomotionType.arcWidth, locomotionType.arcFadeDuration);
        }
        /// <summary>
        /// 
        /// </summary>
        private void CastStay()
        {
            controllerProxy.transform.LerpTransforms(XRInputController.Transform(locomotionHand), locomotionType.smoothing);
            
            arcPoints.Clear();
            
            if (locomotionType.arcRadius < controllerProxy.position.y)
            {
                locomotionType.arcRadius = controllerProxy.position.y + 0.02f;
            }
            
            Vector3 position = controllerProxy.position;
            Vector3 forward = controllerProxy.forward;

            // Get the angle of the controller where pointing the ground is 0 and increasing while going from pointing the ground to the sky.
            float initialAngle = (Mathf.Deg2Rad * controllerProxy.localEulerAngles.x) + (Mathf.PI / 2);
            initialAngle = initialAngle > Mathf.PI ? initialAngle - (Mathf.PI * 2) : initialAngle;
            initialAngle = Mathf.PI - initialAngle + OffsetAngle;

            valid = Mathf.Lerp(0, 90, Mathf.InverseLerp(0f, Mathf.PI, initialAngle)) <= locomotionType.cancelAngle;

            // Get the angle of the arc starting from the Stylus and hitting the ground.
            float 
                opposedSide = Mathf.Abs((Mathf.Sin(initialAngle) * locomotionType.arcRadius) - position.y), 
                finalAngle = Mathf.Asin(opposedSide / locomotionType.arcRadius),
                startX = Mathf.Cos(initialAngle) * locomotionType.arcRadius;

            Vector3 beamOrigin = position + (forward * locomotionType.offsetForwardOrigin) + new Vector3(0f, locomotionType.offsetHeightOrigin, 0f);
            arcPoints.Add(beamOrigin);
            
            // Generate the points used in the Bezier curve.
            float beamMaxDistance = (Mathf.Cos(finalAngle) * locomotionType.arcRadius) - startX;
            Vector3 
                controlPoint = beamOrigin + (forward * (beamMaxDistance / 2)),
                endPoint = beamOrigin + (forward * beamMaxDistance);
            endPoint.y = OffsetFromGround;

            float stepping = 1f / (locomotionType.arcResolution - 1);
            int count = 0;

            for (float i = 0f; i < 1f; i += stepping, count++)
            {
                Vector3 
                    firstSegment = Vector3.Lerp(beamOrigin, controlPoint, i),
                    secondSegment = Vector3.Lerp(controlPoint, endPoint, i),
                    curvePoint = Vector3.Lerp(firstSegment, secondSegment, i);
                
                arcPoints.Add(curvePoint);

                if (count > 0 && VolumeIntersection(origin: arcPoints[count - 1], destination: arcPoints[count], out RaycastHit hit, out LocomotionTarget target))
                {
                    volumeTarget = target;
                    locomotionVisual.SetIntersection(hit, target);
                    SetVisual(intersect: true);
                    SetArc(intersect: true);
                    return;
                }
                else
                {
                    volumeTarget = null;
                }
            }
            SetVisual(intersect: false);
            SetArc(intersect: false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intersect"></param>
        private void SetVisual(bool intersect)
        {
            locomotionVisual.Valid(valid);
            locomotionVisual.Intersecting(intersect);
            locomotionVisual.SetPosition(arcPoints[arcPoints.Count - 1]);
            SetRotation();
            locomotionVisual.SetRotation(visualForward, visualRotation);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SetRotation()
        {
            visualForward = XRInputController.Forward(locomotionHand, true);
            switch (locomotionType.locomotionDirectionMethod)
            {
                case LocomotionType.LocomotionDirectionMethod.ControllerForward:
                    break;
                case LocomotionType.LocomotionDirectionMethod.JoystickDirection:
                    visualRotation = new Vector3(
                        x: XRInputController.AxisValue(locomotionHand).x, 
                        y: 0,
                        z: XRInputController.AxisValue(locomotionHand).y);
                    break;
                case LocomotionType.LocomotionDirectionMethod.TwistRotation:
                    visualRotation = XRInputController.Rotation(locomotionHand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// 
        /// </summary>ub
        private void SetArc(bool intersect)
        {
            if (intersect && locomotionType.additionalArcPoints)
            {
                foreach (Transform arcTransform in locomotionVisual.ArcTransforms())
                {
                    arcPoints.Add(arcTransform.position);
                }
            }
            locomotionArc.Width(valid ? locomotionType.arcWidth : locomotionType.arcWidth * locomotionType.inactiveArcWidthModifier);
            locomotionArc.positionCount = arcPoints.Count;
            locomotionArc.SetPositions(arcPoints.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        private void CastEnd()
        {
            XRInteractionController.SetAllowedState(locomotionHand, true);
            cooldown = true;
            locomotionVisual.SetRotation(visualForward, visualRotation);
            DisableArc();
            locomotionGhost.DisableGhost();
            locomotionVisual.DisableVisual();
        }
        /// <summary>
        /// 
        /// </summary>
        private void DisableArc()
        {
            dynamicArcWidth = 0f;
            cooldown = false;
            casting = false;
            locomotionArc.enabled = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool VolumeIntersection(Vector3 origin, Vector3 destination, out RaycastHit intersection, out LocomotionTarget target)
        {
            if (Physics.Raycast(
                origin: origin,
                direction: destination - origin,
                hitInfo: out RaycastHit hit,
                maxDistance: Vector3.Distance(destination, origin)) &&
                hit.transform.TryGetComponent(out LocomotionTarget validTarget))
            {
                intersection = hit;
                target = validTarget;
                return true;
            }
            intersection = new RaycastHit();
            target = null;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        private bool Valid()
        {
            return !casting && !gesture && !cooldown && !teleporting;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="check"></param>
        public void SetExternalTriggerState(bool state, Check check = Check.Head)
        {
            externalTrigger = state;
            externalTriggerHand = check;
        }
    }
}