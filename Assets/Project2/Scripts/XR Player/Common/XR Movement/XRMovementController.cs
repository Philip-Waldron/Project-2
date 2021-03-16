using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRMovementController : XRInputAbstraction
    {
        [Header("Movement Settings")]
        [SerializeField, Range(0f, 100f)] private float movementForce = 25f;
        [SerializeField, Range(float.Epsilon, 1f)] public float attachDuration = .5f, detachDuration = .2f;
        [SerializeField, Range(0f, 1f)] private float hipOffset = .15f, headOffset = .75f;
        [SerializeField, Range(float.Epsilon, .1f)] private float magnetWidth = .025f, finderWidth = .015f;
        [Space(10f)]
        [SerializeField, Range(0f, 1000f)] public float maximumDistance = 250f;
        [SerializeField, Range(0f, 180f)] public float devianceTolerance = 30f;
        [SerializeField, Range(0f, 1f)] public float finderDamping = .75f, magnetDamping = .5f;
        [SerializeField] private XRInputController.XRControllerButton attach = XRInputController.XRControllerButton.Grip, move = XRInputController.XRControllerButton.Trigger;
        
        [Header("Movement References")] 
        [SerializeField] public GameObject finderAnchorVisual; 
        [SerializeField] private Material magnetMaterial, finderMaterial;
        
        private GameObject movementParent;
        private XRMovementInformation left, right;

        public Rigidbody PlayerRigidbody => GetComponent<Rigidbody>();

        private void Awake()
        {
            movementParent = Set.Object(gameObject, "[Movement Parent]", Vector3.zero);
            left = gameObject.AddComponent<XRMovementInformation>();
            right = gameObject.AddComponent<XRMovementInformation>();
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
            movementParent.transform.position = XRInputController.NormalisedPosition(XRInputController.Check.Head);
            movementParent.transform.eulerAngles = XRInputController.NormalisedRotation(XRInputController.Check.Head);
            left.SetTransform(-hipOffset, heightOffset: headOffset);
            right.SetTransform(hipOffset, heightOffset: headOffset);
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
            if (Physics.Raycast(movementInformation.CastOriginPosition, movementInformation.CastVector, out RaycastHit hit, maximumDistance) && hit.transform.CompareTag("CanAttach"))
            {
                movementInformation.ValidCurrentAnchorPoint(hit);    
            }
            else
            {
                movementInformation.NoValidCurrentAnchorPoint();
            }
            
            movementInformation.FinderVisual();
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
            if (movementInformation.Attached && XRInputController.ControllerButton(move, movementInformation.check))
            {
                PlayerRigidbody.AddForce(movementInformation.MoveVector * movementForce, ForceMode.Acceleration);
                Debug.DrawRay(movementInformation.CastOriginPosition, movementInformation.MoveVector * movementForce, Color.red);
            }
        }
    }
}
