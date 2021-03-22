using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Project2.Scripts.Game_Logic;
using Project2.Scripts.Interactives;
using Project2.Scripts.Interfaces;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using XR_Prototyping.Scripts.Utilities.Extensions;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRPlayerController : MonoBehaviour
    {
        public static XRPlayerController Instance { get; private set; }
        
        public enum GravityBehaviour
        {
            AlwaysDisableGravity,
            DisableGravityWhenMoving,
            AlwaysEnableGravity
        }
        public enum MovementBehaviour
        {
            MoveWhenAnchored,
            MoveWhenButtonPressed
        }

        #region [Inspector and Variables]
        
        // todo, make this a scriptable object
        [Header("Movement Settings")]
        [SerializeField, Range(0f, 100f)] public float movementForce = 15f;
        public GravityBehaviour PlayerGravityBehaviour { get; private set; } = GravityBehaviour.DisableGravityWhenMoving;
        [SerializeField, Range(0f, 100f)] private float mass = 60f, drag = .5f; 
        // -------------------------------------------------------------------------------------------------------------
        [Header("Magnet Anchoring Animation Settings")]
        [SerializeField, Range(float.Epsilon, 1f)] public float attachDuration = .5f;
        [SerializeField, Range(float.Epsilon, 1f)] public float detachDuration = .2f;
        // -------------------------------------------------------------------------------------------------------------
        [Header("Cast Origin Position Settings")]
        [SerializeField, Range(0f, 1f)] public float hipOffset = .15f;
        [SerializeField, Range(0f, 1f)] public float headOffset = .5f;
        [SerializeField] public GameObject castOrigin;
        // -------------------------------------------------------------------------------------------------------------
        [Header("Visual Settings")]
        [SerializeField] public GameObject finderAnchorVisual; 
        [SerializeField] private Material magnetMaterial, finderMaterial;
        [SerializeField, Range(float.Epsilon, .1f)] private float 
            magnetWidth = .05f, 
            finderWidth = .025f;
        public Color 
            magnetAnchorColour = new Color(1,1,1,1),
            magnetGrabColour = new Color(1,1,1,1),
            validAnchorColour = new Color(1,1,1,1),
            invalidAnchorColour = new Color(1,1,1,1),
            searchingAnchorColour = new Color(1,1,1,1);
        // -------------------------------------------------------------------------------------------------------------
        [Header("Cast Location Settings")]
        [SerializeField, Range(0f, 1000f)] public float maximumInteractionDistance = 250f;
        [SerializeField, Range(0f, 180f)] public float validPointDeviationLimit = 30f;
        [SerializeField, Range(0f, 1f)] public float finderDamping = .75f, magnetDamping = .5f;
        public MovementBehaviour PlayerMovementBehaviour { get; private set; } = MovementBehaviour.MoveWhenAnchored;
        public XRInputController.XRControllerButton 
            anchorTrigger = XRInputController.XRControllerButton.Grip,
            moveTrigger = XRInputController.XRControllerButton.Trigger;
        // -------------------------------------------------------------------------------------------------------------
        // Private variables
        private GameObject interactionParent;
        private XRInteractionInformation 
            left, 
            right;
        // -------------------------------------------------------------------------------------------------------------
        // Locally set publicly accessible variables
        public Rigidbody PlayerRigidbody { get; private set; }

        #endregion
        #region [Initialisation and Setup]

        private void Awake()
        {
            CreateSingleton();
            SetupPlayerRigidbody();
            SetupInteractionInformation();
        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateSingleton()
        {
            if (Instance != null)
            {
                Debug.LogError(message: $"There is already an XRPlayerController in {SceneManager.GetActiveScene().name} → {Instance.name}");
                Debug.LogError(message: $"<b>Destroying {name}</b>");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Debug.LogError(message: $"XRPlayerController created in {SceneManager.GetActiveScene().name} → {Instance.name}");
            }
        }
        /// <summary>
        /// Ensure that the rigidbody is always configured correctly on load
        /// </summary>
        private void SetupPlayerRigidbody()
        {
            PlayerRigidbody = gameObject.AddConfiguredRigidbody(mass, drag, ShouldPlayerUseGravity());
            Debug.Log($"Player's gravity set to <b>{PlayerRigidbody.useGravity}</b> → [{PlayerGravityBehaviour.ToString()}]");
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetupInteractionInformation()
        {
            interactionParent = Set.Object(parent: null, name: "[XRPlayer] Interaction Parent", transform.position);
            SetupInteractionInformation(left, XRInputController.Check.Left);
            SetupInteractionInformation(right, XRInputController.Check.Right);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interactionInformation"></param>
        /// <param name="setCheck"></param>
        private void SetupInteractionInformation([NotNull] XRInteractionInformation interactionInformation, XRInputController.Check setCheck)
        {
            if (interactionInformation == null) throw new ArgumentNullException(nameof(interactionInformation));
            interactionInformation = interactionParent.AddComponent<XRInteractionInformation>();
            interactionInformation.InitialiseInteractionInformation(setCheck);
        }

        #endregion
        
        private void Update()
        {
            SetAnchorTransformationInformation();
        }
        private Vector3 HeadOffsetPosition => new Vector3(
            x: XRInputController.Instance.Position(XRInputController.Check.Head).x, 
            y: XRInputController.Instance.Position(XRInputController.Check.Head).y - headOffset, 
            z: XRInputController.Instance.Position(XRInputController.Check.Head).z);
        /// <summary>
        /// 
        /// </summary>
        private void SetAnchorTransformationInformation()
        {
            interactionParent.transform.position = HeadOffsetPosition;
            interactionParent.transform.eulerAngles = XRInputController.Instance.NormalisedRotation(XRInputController.Check.Head);
            left.SetCastOriginTransform(-hipOffset);
            right.SetCastOriginTransform(hipOffset);
        }
        // -------------------------------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            CheckGravityState();
        }
        private bool AnchoredAndMoving() => left.AnchoredAndMoving || right.AnchoredAndMoving;
        /// <summary>
        /// Determine what the gravity should be when the player is not anchored to anything
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool ShouldPlayerUseGravity()
        {
            switch (PlayerGravityBehaviour)
            {
                case GravityBehaviour.AlwaysDisableGravity:
                    return false;
                case GravityBehaviour.DisableGravityWhenMoving when AnchoredAndMoving():
                    return false;
                case GravityBehaviour.DisableGravityWhenMoving:
                    return true;
                case GravityBehaviour.AlwaysEnableGravity:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Set the state of the player's gravity depending on the defined behaviour
        /// </summary>
        private void CheckGravityState()
        {
            PlayerRigidbody.useGravity = ShouldPlayerUseGravity();
        }
    }
}
