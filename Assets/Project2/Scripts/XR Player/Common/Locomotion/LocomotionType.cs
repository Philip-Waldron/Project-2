using UnityEngine;

namespace XR_Prototyping.Scripts.Common.Locomotion
{
    [CreateAssetMenu(fileName = "LocomotionType", menuName = "XR Prototyping/Locomotion", order = 1)]
    public class LocomotionType : ScriptableObject
    {
        public enum LocomotionVisualEffect
        {
            Dash,
            Blink
        }
        public enum BackwardsGestureEffect
        {
            StepBack,
            TurnAround
        }
        public enum LocomotionDirectionMethod
        {
            ControllerForward,
            JoystickDirection,
            TwistRotation
        }
        public enum LocomotionStartTrigger
        {
            JoystickMovement,
            TouchstripTouch,
            TouchstripSingleTap,
            TouchstripLongPress,
            ExternalTrigger
        }
        public enum LocomotionEndTrigger
        {
            JoystickRecenter,
            TouchstripRelease,
            TouchstripTouch,
            ExternalTrigger
        }
        public enum LocomotionHandedness
        {
            DominantOnly,
            NonDominantOnly,
            Both
        }
        [Header("Locomotion Settings")]
        [SerializeField] public LocomotionVisualEffect locomotionVisualEffect = LocomotionVisualEffect.Dash;
        [SerializeField] public LocomotionHandedness locomotionHandedness = LocomotionHandedness.NonDominantOnly;
        [SerializeField] public BackwardsGestureEffect backwardsGestureEffect = BackwardsGestureEffect.StepBack;
        [SerializeField] public LocomotionDirectionMethod locomotionDirectionMethod = LocomotionDirectionMethod.JoystickDirection;
        [SerializeField] public LocomotionStartTrigger locomotionStartTrigger = LocomotionStartTrigger.JoystickMovement;
        [SerializeField] public LocomotionEndTrigger locomotionEndTrigger = LocomotionEndTrigger.TouchstripRelease;
        [SerializeField, Range(0f, 5f)] public float stepBackDistance = .5f, dashDuration = 1.5f, gestureDuration = 1f;
        [SerializeField, Range(0, 90f)] public float snapRotationAngle = 45f, cancelAngle = 60f;
        [SerializeField, Range(1, 5)] public int twistMultiplier = 1;
        [SerializeField, Range(0f, 1f)] public float vignetteStrength = 1f;
        [Header("Arc Settings")]
        [SerializeField] public Material arcMaterial;
        [SerializeField, Range(0.001f, 0.01f)] public float arcWidth = 0.005f;
        [SerializeField, Range(1, 0)] public float inactiveArcWidthModifier = .25f;
        [SerializeField, Range(2, 64)] public int arcResolution = 32;
        [SerializeField, Range(0f, 20f)] public float arcRadius = 10f;
        [SerializeField, Range(0f, 1f)] public float smoothing = .75f, offsetForwardOrigin = 0.01f,  offsetHeightOrigin = 0f;
        [Range(0f, 1f)] public float arcFadeDuration = .015f;
        [SerializeField] public bool additionalArcPoints = true;
        [Header("References")]
        [SerializeField] public GameObject locomotionVisualPrefab;
        [SerializeField] public GameObject locomotionGhostPrefab;

    }
}
