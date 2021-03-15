using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.Locomotion
{
    public class LocomotionVisual : MonoBehaviour
    {
        [SerializeField] private Transform visual, axis, intersection;
        [SerializeField] private Image visualImage, axisImage, intersectionImage, locomotionTarget;
        [SerializeField] private Color visualColor = new Color(0,0,0,1);
        [SerializeField] private bool showTarget = true;
        [SerializeField, Range(0,1)] private float fadeMultiplier = .5f, intersectionSmoothing = .75f;

        private LocomotionTarget currentTarget;
        private XR_Prototyping.Scripts.Common.Locomotion.Locomotion locomotion;
        private Color VisualColor => new Color(visualColor.r, visualColor.g, visualColor.b, visualAlpha);
        private Color IntersectionColor => new Color(visualColor.r, visualColor.g, visualColor.b, intersectionAlpha);

        private Transform intersectionPoint, offsetParent, offset, visualParent, target;
        private readonly List<Transform> arcTransforms = new List<Transform>();
        private bool intersecting, previousIntersecting, intersected;
        private float visualAlpha, intersectionAlpha, volumeTargetOffset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cachedLocomotion"></param>
        public void SetupVisual(Locomotion cachedLocomotion)
        {
            locomotion = cachedLocomotion;
            intersectionPoint = Set.Object(gameObject, "[Intersection Transform]", Vector3.zero).transform;
            offsetParent = Set.Object(gameObject, "[Offset Parent]", Vector3.zero).transform;
            offset = Set.Object(offsetParent.gameObject, "[Offset]", Vector3.zero).transform;
            visualParent = Set.Object(gameObject, "[Visual Parent]", Vector3.zero).transform;
            target = Set.Object(visualParent.gameObject, "[Target]", Vector3.zero).transform;

            visual.SetParent(visualParent);
            
            arcTransforms.Add(intersectionPoint);
            arcTransforms.Add(offsetParent);
            arcTransforms.Add(offset);
            
            SetTargetState(false);
        }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            visualImage.color = VisualColor;
            axisImage.color = VisualColor;
            intersectionImage.color = IntersectionColor;
        }
        /// <summary>
        /// 
        /// </summary>
        public void EnableVisual()
        {
            DOTween.To(() => visualAlpha, x => visualAlpha = x, 1f, locomotion.locomotionType.arcFadeDuration * fadeMultiplier);
        }
        /// <summary>
        /// 
        /// </summary>
        public void DisableVisual()
        {
            DOTween.To(() => visualAlpha, x => visualAlpha = x, 0f, locomotion.locomotionType.arcFadeDuration * fadeMultiplier);
            IntersectionEnd();
        }
        /// <summary>
        /// 
        /// </summary>
        private void IntersectionStart()
        {
            intersected = true;
            DOTween.To(() => intersectionAlpha, x => intersectionAlpha = x, 1f, locomotion.locomotionType.arcFadeDuration * fadeMultiplier);
            SetOffsetPosition(intersectionPoint.position);
        }
        /// <summary>
        /// 
        /// </summary>
        private void IntersectionStay()
        {
            if (!intersected)
            {
                IntersectionStart();
            }
            SetOffsetPosition(Vector3.Lerp(intersection.position, intersectionPoint.position, intersectionSmoothing));
        }
        /// <summary>
        /// 
        /// </summary>
        private void IntersectionEnd()
        {
            intersectionAlpha = 0f;
            intersected = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {
            transform.position = intersecting ? offset.position : position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="rotationInformation"></param>
        public void SetRotation(Vector3 forward, Vector3 rotationInformation)
        {
            axis.rotation = Quaternion.identity;
            Vector3 offsetForward = -offset.forward;
            
             
            if (intersecting)
            {
                visualParent.forward = offsetForward;
                visual.localRotation = Quaternion.identity;
            }
            else
            {
                visualParent.forward = forward;
                switch (locomotion.locomotionType.locomotionDirectionMethod)
                {
                    case LocomotionType.LocomotionDirectionMethod.ControllerForward:
                        break;
                    case LocomotionType.LocomotionDirectionMethod.JoystickDirection:
                        target.localPosition = rotationInformation;
                        visual.LookAt(target);
                        break;
                    case LocomotionType.LocomotionDirectionMethod.TwistRotation:
                        visual.localEulerAngles = new Vector3(0, -(rotationInformation.z * locomotion.locomotionType.twistMultiplier), 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void TeleportInformation(out Vector3 position, out Vector3 rotation)
        {
            position = transform.position;
            rotation = new Vector3(0f, visual.eulerAngles.y, 0f);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetOffsetPosition(Vector3 position)
        {
            switch (currentTarget.targetType)
            {
                case LocomotionTarget.TargetType.Volume:
                    intersection.position = position;
                    offsetParent.position = Set.NormalisedPosition(intersection.position);
                    offsetParent.forward = intersection.forward;
                    offset.localPosition = new Vector3(0, 0, volumeTargetOffset);
                    break;
                case LocomotionTarget.TargetType.Point:
                    currentTarget.PointTarget(out Vector3 pointPosition, out Vector3 pointForward);
                    intersection.position = position;
                    offsetParent.position = Set.NormalisedPosition(pointPosition);
                    offsetParent.forward = pointForward;
                    offset.localPosition = Vector3.zero;
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetIntersection(RaycastHit hit, LocomotionTarget hitTarget)
        {
            switch (hitTarget.targetType)
            {
                case LocomotionTarget.TargetType.Volume:
                    intersectionPoint.position = hit.point;
                    intersection.forward = hit.normal;
                    currentTarget = hitTarget;
                    volumeTargetOffset = currentTarget.Offset();
                    break;
                case LocomotionTarget.TargetType.Point:
                    intersectionPoint.position = hit.point;
                    intersection.forward = hit.normal;
                    currentTarget = hitTarget;
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Intersecting(bool validIntersection)
        {
            intersecting = validIntersection;
            if (intersecting && !previousIntersecting)
            {
                IntersectionStart();
            }
            if (intersecting)
            {
                IntersectionStay();
            }
            if (!intersecting && previousIntersecting)
            {
                IntersectionEnd();
            }
            previousIntersecting = intersecting;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Valid(bool valid)
        {
            visualAlpha = valid ? 1f : .1f;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Transform> ArcTransforms()
        {
            return arcTransforms;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Transform GhostParent()
        {
            return visual;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetTargetState(bool state)
        {
            if (!showTarget) return;
            locomotionTarget.enabled = state;
            Transform targetTransform = locomotionTarget.transform;
            if (state)
            {
                targetTransform.SetParent(null);
                targetTransform.Position(visual);
            }
            else
            {
                targetTransform.SetParent(visual);
                targetTransform.localPosition = Vector3.zero;
            }
        }
    }
}
