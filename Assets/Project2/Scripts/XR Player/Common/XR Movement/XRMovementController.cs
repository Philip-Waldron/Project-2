using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRMovementController : XRInputAbstraction
    {
        [Header("Movement Force Settings")]
        public bool UseMagneticForce;
        [SerializeField, Range(0f, 100f)] public float MagneticForce = 50f;
        public bool UseCastForce;
        [SerializeField, Range(0f, 100f)] public float CastForce = 15f;
        public bool UseManoeuvreForce;
        [SerializeField, Range(0f, 100f)] public float ManoeuvreForce = 15f;
        public bool UseAverageForce;
        [SerializeField, Range(0f, 100f)] public float AverageForce = 15f;
        public bool DisableGravityOnForceApplied;
        [Header("Magnet Animation Settings")]
        [SerializeField, Range(float.Epsilon, 1f)] public float attachDuration = .5f;
        [SerializeField, Range(float.Epsilon, 1f)] public float detachDuration = .2f;
        [Header("Cast Origin Position Settings")]
        [SerializeField, Range(0f, 1f)] public float hipOffset = .15f;
        [SerializeField, Range(0f, 1f)] public float headOffset = .5f;
        [Header("Visual Settings")]
        [SerializeField] public GameObject finderAnchorVisual;
        [SerializeField] private Material magnetMaterial, finderMaterial;
        [SerializeField, Range(float.Epsilon, .1f)] private float magnetWidth = .025f, finderWidth = .015f;
        [Header("Cast Location Settings")]
        [SerializeField, Range(0f, 1000f)] public float maximumDistance = 250f;
        [SerializeField, Range(0f, 180f)] public float devianceTolerance = 30f;
        [SerializeField, Range(0f, 1f)] public float finderDamping = .75f, magnetDamping = .5f;

        [SerializeField] private XRInputController.XRControllerButton attach = XRInputController.XRControllerButton.Grip, move = XRInputController.XRControllerButton.Trigger;

        private GameObject movementParent;
        public XRMovementInformation left, right;

        public Rigidbody PlayerRigidbody => GetComponent<Rigidbody>();

        private void Awake()
        {
            movementParent = Set.Object(null, "[Movement Parent]", Vector3.zero);
            left = movementParent.AddComponent<XRMovementInformation>();
            right = movementParent.AddComponent<XRMovementInformation>();
            left.SetupMovementInformation(this, movementParent,  XRInputController.Check.Left, magnetMaterial, magnetWidth, finderMaterial, finderWidth);
            right.SetupMovementInformation(this, movementParent, XRInputController.Check.Right, magnetMaterial, magnetWidth, finderMaterial, finderWidth);
        }

        private void Update()
        {
            SetTransforms();
            FindValidAnchors();
            CheckStates();
        }

        private void SetTransforms()
        {
            Vector3 position = new Vector3(
                XRInputController.Position(XRInputController.Check.Head).x,
                XRInputController.Position(XRInputController.Check.Head).y - headOffset,
                XRInputController.Position(XRInputController.Check.Head).z);
            movementParent.transform.position = position;
            movementParent.transform.eulerAngles = XRInputController.NormalisedRotation(XRInputController.Check.Head);

            left.SetTransform(-hipOffset);
            right.SetTransform(hipOffset);
        }

        private void FindValidAnchors()
        {
            FindValidAnchor(left);
            FindValidAnchor(right);
        }

        private void CheckStates()
        {
            AttachDetach(left);
            AttachDetach(right);

            MoveToAnchor(left);
            MoveToAnchor(right);
        }

        private void FindValidAnchor(XRMovementInformation movementInformation)
        {
            Color debug = Color.red;
            float distance = maximumDistance;

            if (Physics.Raycast(movementInformation.CastOriginPosition, movementInformation.CastVector, out RaycastHit hit, maximumDistance) && hit.transform.CompareTag("CanAttach"))
            {
                movementInformation.ValidCurrentAnchorPoint(hit);
                debug = Color.green;
                distance = hit.distance;
            }
            else
            {
                movementInformation.NoValidCurrentAnchorPoint();
            }

            movementInformation.DrawVisuals();

            Debug.DrawRay(movementInformation.CastOriginPosition, movementInformation.CastVector * distance, debug);
        }

        private void AttachDetach(XRMovementInformation movementInformation)
        {
            if (!movementInformation.Attached && XRInputController.InputEvent(attach).State(movementInformation.check, XRInputController.InputEvents.InputEvent.Transition.Down))
            {
                movementInformation.TriggerAttach();
            }
            if (XRInputController.InputEvent(attach).State(movementInformation.check, XRInputController.InputEvents.InputEvent.Transition.Up))
            {
                movementInformation.TriggerDetach();
            }
        }

        private void MoveToAnchor(XRMovementInformation movementInformation)
        {
            if (XRInputController.ControllerButton(move, movementInformation.check))
            {
                movementInformation.MoveToAnchor();

                if (DisableGravityOnForceApplied)
                {
                    PlayerRigidbody.useGravity = !movementInformation.Attached;
                }
            }
            else
            {
                if (DisableGravityOnForceApplied)
                {
                    PlayerRigidbody.useGravity = true;
                }
            }
        }
    }
}
