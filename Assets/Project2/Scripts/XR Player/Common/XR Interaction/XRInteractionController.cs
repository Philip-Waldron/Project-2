using System;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;
using Information = XR_Prototyping.Scripts.Utilities.IndirectInteraction.InteractionInformation;

namespace XR_Prototyping.Scripts.Common.XR_Interaction
{
    public class XRInteractionController : XRInputAbstraction
    {
        [Header("Interaction Settings")]
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField, Range(0f, 10f)] private float maximumIndirectDistance = 5f, nearInteractionDistance = .25f;
        [SerializeField, Range(0f, 0.005f)] private float raycastWidth = 0.001f;
        [Header("References")]
        [SerializeField] private Material raycastMaterial;
        [SerializeField] private GameObject nearInteractionVisual;

        private const float MinimumDistance = float.Epsilon;

        private Elements.InteractionElement
            leftInteractionElement,
            rightInteractionElement;

        private void Start()
        {
            GameObject interactionController = gameObject;
            
            leftInteractionElement = interactionController.AddComponent<Elements.InteractionElement>();
            rightInteractionElement = interactionController.AddComponent<Elements.InteractionElement>();
            
            leftInteractionElement.InitialiseInteractionElement(
                handedness: Check.Left,
                proxyTransform: Set.Object(interactionController, "[Interaction] Left", Vector3.zero).transform,
                raycastMaterial, raycastWidth, nearInteractionVisual);
            rightInteractionElement.InitialiseInteractionElement(
                handedness: Check.Right,
                proxyTransform: Set.Object(gameObject, "[Interaction] Right", Vector3.zero).transform,
                raycastMaterial, raycastWidth, nearInteractionVisual);
        }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            CheckInteraction();
        }
        /// <summary>
        /// This checks if either controller is hovering over an interactable interface element,
        /// will return a RaycastHit if it is, and contains the logic for triggering interactions
        /// and returns true if either controller is valid
        /// </summary>
        private void CheckInteraction()
        {
            if (IndirectInteraction.InteractionHover(
                out bool left, out bool right, 
                out Information leftInformation, out Information rightInformation, 
                interactionLayer, maximumIndirectDistance, MinimumDistance))
            {
                leftInteractionElement.Engaged = left;
                rightInteractionElement.Engaged = right;
                if (left && leftInteractionElement.Allowed)
                {
                    leftInteractionElement.Information = leftInformation;
                    Elements.InteractionElement.InteractionLogic(leftInteractionElement, leftInformation);
                }
                else
                {
                    leftInteractionElement.SetDefaultState();
                }
                if (right && rightInteractionElement.Allowed)
                {
                    rightInteractionElement.Information = rightInformation;
                    Elements.InteractionElement.InteractionLogic(rightInteractionElement, rightInformation);
                }
                else
                {
                    rightInteractionElement.SetDefaultState();
                }
            }
            else
            {
                leftInteractionElement.SetDefaultState();
                rightInteractionElement.SetDefaultState();
            }
            
            CheckState(Check.Right, rightInteractionElement.currentXRInterface, rightInteractionElement.previousXRInterface);
            rightInteractionElement.previousXRInterface = rightInteractionElement.currentXRInterface;

            CheckState(Check.Left, leftInteractionElement.currentXRInterface, leftInteractionElement.previousXRInterface);
            leftInteractionElement.previousXRInterface = leftInteractionElement.currentXRInterface;
        }
        /// <summary>
        /// Allows other scripts to disable interactions
        /// </summary>
        /// <param name="check"></param>
        /// <param name="state"></param>
        public void SetAllowedState(Check check, bool state)
        {
            GetInteractionElement(check).Allowed = state;
        }
        /// <summary>
        /// Allows other scripts to check to see if a particular interaction set is disabled
        /// </summary>
        /// <param name="check"></param>
        public bool GetInteractionState(Check check)
        {
            return GetInteractionElement(check).Allowed;
        }
        /// <summary>
        /// Allows other scripts to check to see if you are engaged with any interfaces
        /// </summary>
        /// <param name="check"></param>
        public bool GetEngagedState(Check check)
        {
            return GetInteractionElement(check).Engaged;
        }
        /// <summary>
        /// Returns false if the queried hand is disabled, or engaged with a UI element
        /// </summary>
        /// <param name="check"></param>
        public bool QueryInteractionAvailability(Check check)
        {
            return GetInteractionElement(check).Allowed && !GetInteractionElement(check).Engaged;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public Information GetInteractionInformation(Check check)
        {
            return GetInteractionElement(check).Information;
        }
        public static void CheckState(Check check, XRInterfaceAbstraction current, XRInterfaceAbstraction previous, bool immediate = false)
        {
            if (current != null)
            {
                if (current == previous) // You are interacting with the same interface element
                {
                    current.EngageStay(check, immediate);
                }
                else // You are interacting with a new interface element
                {
                    current.EngageStart(check, immediate); 
                    
                    if (previous != null)
                    {
                        previous.EngageEnd(check, immediate);
                    }
                }
            }
            else if (previous != null)
            {
                previous.EngageEnd(check, immediate);
            }
        }
        public void SetXRInteractionOrigin(XRInteractionOrigin origin, Check check)
        {
            GetInteractionElement(check).SetInteractionOrigin(origin);
        }
        public XRInteractionOrigin GetXRInteractionOrigin(Check check)
        {
            return GetInteractionElement(check).GetInteractionOrigin();
        }

        private Elements.InteractionElement GetInteractionElement(Check check)
        {
            switch (check)
            {
                case Check.Left:
                    return leftInteractionElement;
                case Check.Right:
                    return rightInteractionElement;
                case Check.Head:
                    return null;
                default:
                    return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool InteractionInitialised(Check check)
        {
            return GetInteractionElement(check).Initialised;
        }
        /// <summary>
        /// Robust way of checking whether to distinguish between direct, near, and indirect interactions
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool NearInteraction(float distance)
        {
            return distance < nearInteractionDistance;
        }
        /// <summary>
        /// Robust way of checking whether to distinguish between direct, near, and indirect interactions
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public float NearDistanceScale(float distance)
        {
            return Mathf.InverseLerp(nearInteractionDistance, float.Epsilon, distance);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float MaximumDistance()
        {
            return maximumIndirectDistance;
        }
    }
}
