using Project2.Scripts.Game_Logic;
using Project2.Scripts.Interactives;
using Project2.Scripts.Interfaces;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRInteractionController : XRInputAbstraction
    {
        [Header("Movement Force Settings")]
        // public bool UseMagneticForce;
        // [SerializeField, Range(0f, 100f)] public float MagneticForce = 50f;
        // public bool UseCastForce;
        // [SerializeField, Range(0f, 100f)] public float CastForce = 15f;
        // public bool UseManoeuvreForce;
        // [SerializeField, Range(0f, 100f)] public float ManoeuvreForce = 15f;
        // public bool UseAverageForce;
        
        [SerializeField, Range(0f, 100f)] public float averageForce = 15f;
        public bool DisableGravityOnForceApplied;
        [Header("Magnet Animation Settings")]
        [SerializeField, Range(float.Epsilon, 1f)] public float attachDuration = .5f;
        [SerializeField, Range(float.Epsilon, 1f)] public float detachDuration = .2f;
        [Header("Cast Origin Position Settings")]
        [SerializeField, Range(0f, 1f)] public float hipOffset = .15f;
        [SerializeField, Range(0f, 1f)] public float headOffset = .5f;
        [SerializeField] public GameObject castOrigin;
        [Header("Visual Settings")]
        [SerializeField] public GameObject finderAnchorVisual; 
        [SerializeField] private Material magnetMaterial, finderMaterial;
        [SerializeField, Range(float.Epsilon, .1f)] private float magnetWidth = .025f, finderWidth = .015f;
        public Color 
            magnetAnchorColour = new Color(1,1,1,1),
            magnetGrabColour = new Color(1,1,1,1),
            validAnchorColour = new Color(1,1,1,1),
            invalidAnchorColour = new Color(1,1,1,1),
            searchingAnchorColour = new Color(1,1,1,1);
        [Header("Cast Location Settings")]
        [SerializeField, Range(0f, 1000f)] public float maximumDistance = 250f;
        [SerializeField, Range(0f, 180f)] public float devianceTolerance = 30f;
        [SerializeField, Range(0f, 1f)] public float finderDamping = .75f, magnetDamping = .5f;
        [SerializeField] private XRInputController.XRControllerButton attach = XRInputController.XRControllerButton.Grip, move = XRInputController.XRControllerButton.Trigger;
        [Header("Manipulation Settings")]
        [SerializeField, Range(1f, 5f)] public float lassoOffset;
        [SerializeField, Range(1f, 5f)] public float magneticGrabForce = 3f;
        
        private GameObject interactionParent;
        private bool useGravity;
        private XRInteractionInformation left, right;

        public Rigidbody PlayerRigidbody { get; private set; }

        private void Awake()
        {
            PlayerRigidbody = GetComponent<Rigidbody>();
            
            interactionParent = Set.Object(null, "[Movement Parent]", Vector3.zero);
            
            left = interactionParent.AddComponent<XRInteractionInformation>();
            right = interactionParent.AddComponent<XRInteractionInformation>();

            left.SetupMovementInformation(this, interactionParent,  XRInputController.Check.Left, magnetMaterial, magnetWidth, finderMaterial, finderWidth);
            right.SetupMovementInformation(this, interactionParent, XRInputController.Check.Right, magnetMaterial, magnetWidth, finderMaterial, finderWidth);

            useGravity = PlayerRigidbody.useGravity;
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
                XRInputController.Instance.Position(XRInputController.Check.Head).x, 
                XRInputController.Instance.Position(XRInputController.Check.Head).y - headOffset, 
                XRInputController.Instance.Position(XRInputController.Check.Head).z);
            interactionParent.transform.position = position;
            interactionParent.transform.eulerAngles = XRInputController.Instance.NormalisedRotation(XRInputController.Check.Head);
            
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

        private void FindValidAnchor(XRInteractionInformation interactionInformation)
        {
            Color debug = Color.red;
            float distance = maximumDistance;

            if (Physics.Raycast(interactionInformation.CastOriginPosition, interactionInformation.CastVector, out RaycastHit hit, maximumDistance))
            {
                if (hit.transform.TryGetComponent(out InteractiveObjectInterfaces.ICanAttach canAttach)/*.transform.CompareTag("CanAttach")*/)
                {
                    interactionInformation.ValidCurrentAnchorPoint(hit, true, false);
                    debug = Color.green;
                    distance = hit.distance;   
                }
                else if (hit.transform.TryGetComponent(out InteractiveObjectInterfaces.ICanGrab canGrab)/*.CompareTag("CanGrab")*/)
                {
                    interactionInformation.ValidCurrentAnchorPoint(hit, false, true);
                    debug = Color.yellow;
                    distance = hit.distance;
                }
            }
            else
            {
                interactionInformation.NoValidCurrentAnchorPoint(); 
            }
            
            interactionInformation.DrawVisuals();
            
            Debug.DrawRay(interactionInformation.CastOriginPosition, interactionInformation.CastVector * distance, debug);
        }

        private void AttachDetach(XRInteractionInformation interactionInformation)
        {
            if (XRInputController.Instance.InputEvent(attach).State(interactionInformation.check, XRInputController.InputEvents.InputEvent.Transition.Down))
            {
                interactionInformation.TriggerAttach();
            }
            else if (XRInputController.Instance.InputEvent(attach).State(interactionInformation.check, XRInputController.InputEvents.InputEvent.Transition.Up))
            {
                interactionInformation.TriggerDetach(); 
            }
        }
        
        private void MoveToAnchor(XRInteractionInformation movementInformation)
        {
            if (movementInformation.Attached)
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
                    PlayerRigidbody.useGravity = useGravity;
                }
            }
        }
    }
}
