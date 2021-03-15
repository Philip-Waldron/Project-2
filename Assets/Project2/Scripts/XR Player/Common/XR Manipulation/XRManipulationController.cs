using System;
using System.Collections.Generic;
using System.Linq;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;
using Transition = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.InputEvents.InputEvent.Transition;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;
using Event = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Event;

namespace XR_Prototyping.Scripts.Common.XR_Manipulation
{
    public class XRManipulationController : XRInputAbstraction
    {
        private enum ManipulationBehaviour
        {
            ManipulateClosest,
            ManipulateAllWithinRange
        }

        public enum ManipulationHands
        {
            DominantOnly,
            NonDominantOnly,
            Both,
            Neither
        }

        [Header("Interaction Settings")] 
        [SerializeField] private ManipulationHands objectManipulationHands = ManipulationHands.DominantOnly;
        [SerializeField] private ManipulationBehaviour manipulationBehaviour = ManipulationBehaviour.ManipulateClosest;
        [SerializeField, Range(Minimum, Maximum)] private float interactionRange = .005f;
        [SerializeField, Range(0f, 1f)] private float damping = .65f;
        [Header("References")] 
        public Material snapVisualMaterial;

        public bool Cancel { private get; set; }
        public bool Enabled { get; set; } = true;
        private static float CurrentScaleFactor => XRInputController.InterControllerDistance();
        
        private const float Minimum = .01f, Maximum = .2f;
        public const float SnapRadius = 0.5f;

        [Serializable] public class ManipulationInformation
        {
            public Transform proxy;
            public bool manipulating;
            public XRManipulatableElement closestXRManipulatableElement;
            public List<XRManipulatableElement> allValidManipulatableElements = new List<XRManipulatableElement>();
            public List<XRManipulatableElement> manipulatedElements = new List<XRManipulatableElement>();
        }
        [HideInInspector] public ManipulationInformation 
            nonDominantManipulation = new ManipulationInformation(), 
            dominantManipulation = new ManipulationInformation(),
            biManualManipulation = new ManipulationInformation();

        private void Awake()
        {
            nonDominantManipulation.proxy = Set.Object(gameObject, "[Manipulation Proxy] Non-Dominant", Vector3.zero).transform;
            dominantManipulation.proxy = Set.Object(gameObject, "[Manipulation Proxy] Dominant", Vector3.zero).transform;
            biManualManipulation.proxy = Set.Object(gameObject, "[Manipulation Proxy] Bimanual", Vector3.zero).transform;
        }

        private void Update()
        {
            ProxyFollowing();
            switch (objectManipulationHands)
            {
                case ManipulationHands.DominantOnly when Enabled:
                    ManipulationCheck(XRInputController.DominantHand());
                    break;
                case ManipulationHands.NonDominantOnly when Enabled:
                    ManipulationCheck(XRInputController.NonDominantHand());
                    break;
                case ManipulationHands.Both when Enabled:
                    ManipulationCheck(XRInputController.DominantHand());
                    ManipulationCheck(XRInputController.NonDominantHand());
                    break;
                case ManipulationHands.Neither:
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ProxyFollowing()
        {
            GetManipulationInformation(Check.Left).proxy.LerpTransforms(destination: XRInputController.Transform(Check.Left), damping);
            GetManipulationInformation(Check.Right).proxy.LerpTransforms(destination: XRInputController.Transform(Check.Right), damping);
            GetManipulationInformation(Check.Head, bimanual: true).proxy.LerpTransforms(destination: XRInputController.BimanualTransform(), damping);
            GetManipulationInformation(Check.Head, bimanual: true).proxy.ScaleFactor(factor: XRInputController.BimanualTransform().localScale.magnitude, damping);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        private void ManipulationCheck(Check check)
        {
            FindClosestManipulatableElement(check);
            ManipulationLogic(check);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        private void ManipulationLogic(Check check)
        {
            // Cache the relevant manipulation information
            ManipulationInformation manipulationInformation = GetManipulationInformation(check);
            // If you aren't pointing at an interface element, and you grab with the defined hand
            if (!XRInteractionController.GetEngagedState(check) && 
                XRInputController.InputEvent(Event.GripPress).State(
                    check: check, 
                    transition: Transition.Down))
            {
                // You start to manipulate all defined objects
                SetClosestManipulatableElement(check, FindClosestManipulatableElement(check));
                CacheManipulationElements(check, manipulationInformation);
            }
            // If you are manipulating objects already
            else if (manipulationInformation != null && manipulationInformation.manipulating)
            {
                // When you release with the defined hand
                if (XRInputController.InputEvent(Event.GripPress).State(
                    check: check,
                    transition: Transition.Up))
                {
                    // You stop manipulating
                    ManipulateEnd(check, manipulationInformation);
                    // todo add logic for swapping hands
                }
                // When you are just continuing to manipulate objects and aren't bimanually manipulating
                else if (!biManualManipulation.manipulating)
                {
                    // When grabbing an object and you grab with the other hand
                    if (XRInputController.InputEvent(Event.GripPress).State(check: XRInputController.OtherHand(check), transition: Transition.Down))
                    {
                        BimanualManipulationStart(check);
                    }
                    // Continue manipulation
                    ManipulateStay(check, manipulationInformation);
                }
                // When you are bimanually manipulating
                else
                {
                    // Continue manipulation
                    BimanualManipulationStay(check);
                    // When grabbing an object and you grab with the other hand
                    if (XRInputController.InputEvent(Event.GripPress).State(check: XRInputController.OtherHand(check), transition: Transition.Up))
                    {
                        BimanualManipulationEnd(check);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        private void BimanualManipulationStart(Check check)
        {
            // Transfer the manipulated elements to the bimanual proxy
            foreach (XRManipulatableElement element in GetManipulationInformation(check).manipulatedElements)
            {
                element.SetManipulationParent(biManualManipulation.proxy);
            }
            biManualManipulation.manipulating = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        private void BimanualManipulationStay(Check check)
        {
            ManipulateStay(check, GetManipulationInformation(check));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        private void BimanualManipulationEnd(Check check)
        {
            // Set them back into the original proxy
            foreach (XRManipulatableElement element in GetManipulationInformation(check).manipulatedElements)
            {
                element.SetManipulationParent(GetManipulationInformation(check).proxy);
            }
            biManualManipulation.manipulating = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        private XRManipulatableElement FindClosestManipulatableElement(Check check)
        {
            // Cache references
            Vector3 position = XRInputController.Position(check), closestPoint = Vector3.zero;
            float smallestDistance = float.PositiveInfinity;
            XRManipulatableElement closest = null;
            // Clear the list which we are reconstructing
            GetManipulationInformation(check).allValidManipulatableElements.Clear();
            // Predicate for filtering the overlapping colliders
            bool ValidManipulationTarget(Collider overlap) => overlap.TryGetComponent(out XRManipulatableElement manipulatableElement);
            // Find all valid objects in range you can manipulate
            foreach (Collider overlap in Physics.OverlapSphere(position, interactionRange).Where(ValidManipulationTarget))
            {
                overlap.TryGetComponent(out XRManipulatableElement manipulatableElement);
                closestPoint = overlap.ClosestPoint(position);
                float distance = Vector3.Distance(closestPoint, position);
                GetManipulationInformation(check).allValidManipulatableElements.Add(manipulatableElement);
                // Save this information if this is the closest object so far
                if (distance >= smallestDistance) continue;
                closest = manipulatableElement;
                smallestDistance = distance;
            }
            if (!XRDebug.Enabled) return closest;
            XRDebug.DrawSphere($"Manipulation Range {check}", position, interactionRange, Color.red);
            Vector3 from = XRInteractionController.GetXRInteractionOrigin(check).Position();
            XRDebug.DrawLine($"{check} Closest Manipulation", from, closest == null ? from : closestPoint, Color.yellow);
            return closest;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="element"></param>
        private void SetClosestManipulatableElement(Check check, XRManipulatableElement element)
        {
            GetManipulationInformation(check).closestXRManipulatableElement = element;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="bimanual"></param>
        public ManipulationInformation GetManipulationInformation(Check check, bool bimanual = false)
        {
            return 
                bimanual
                ? biManualManipulation
                : check == XRInputController.DominantHand() 
                    ? dominantManipulation
                    : nonDominantManipulation;
        }
        /// <summary>
        /// Logic for deciding what elements you are manipulating
        /// </summary>
        public void CacheManipulationElements(Check check, ManipulationInformation information)
        {
            if (information.closestXRManipulatableElement == null) return;
            
            switch (manipulationBehaviour)
            {
                case ManipulationBehaviour.ManipulateClosest:
                    StartManipulating(information.closestXRManipulatableElement, check, information);
                    break;
                case ManipulationBehaviour.ManipulateAllWithinRange:
                    foreach (XRManipulatableElement element in information.allValidManipulatableElements)
                    {
                        StartManipulating(element, check, information);
                    }
                    break;
                default:
                    return;
            }
            
            information.manipulating = true;
        }
        /// <summary>
        /// The abstracted logic for manipulating individual elements
        /// </summary>
        /// <param name="element"></param>
        /// <param name="check"></param>
        /// <param name="information"></param>
        private void StartManipulating(XRManipulatableElement element, Check check, ManipulationInformation information)
        {
            while (true)
            {
                // If this element is in a group, iterate until you find its parent
                // This way you will always grab the objects root parent
                if (element.IsInGroup)
                {
                    element = element.ManipulatableGroupParent;
                    continue;
                }
                // If the object is already being manipulated 
                if (element.Manipulated)
                {
                    ManipulateEnd(check, information);
                }
                // Stop the object from moving when it's grabbed
                element.Rigidbody.velocity = Vector3.zero;
                // Add it to the list of objects being manipulated by whichever hand
                information.manipulatedElements.Add(element);
                // Start to manipulate the relevant object
                element.ManipulateStart(information.proxy);
                break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ManipulateStay(Check check, ManipulationInformation information)
        {
            // Logic for handling cancellation of manipulation is done here 
            if (Cancel)
            {
                CancelManipulation(check, information);
                Cancel = false;
                return;
            }
            // Otherwise just iterate on each object you are manipulating and call the relevant methods
            foreach (XRManipulatableElement element in information.manipulatedElements)
            {
                element.ManipulateStay();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ManipulateEnd(Check check, ManipulationInformation information)
        {
            // Stop manipulating every object you are manipulating
            foreach (XRManipulatableElement element in information.manipulatedElements)
            {
                StopManipulating(element, check);
            }
            // Reset the state of the manipulation information
            information.manipulatedElements.Clear();
            information.manipulating = false;
            information.closestXRManipulatableElement = null;
            biManualManipulation.manipulating = false;
        }
        /// <summary>
        /// 
        /// </summary>
        private static void StopManipulating(XRManipulatableElement element, Check check)
        {
            // Stop manipulating every grouped object if the object is a group parent
            if (element.GroupParent)
            {
                foreach (XRManipulatableElement child in element.groupChildren)
                {
                    StopManipulating(child, check);
                }
            }
            // Stop manipulating the grabbed object
            element.ManipulateEnd();
            // The object will continue to move with some velocity in the direction the hand is moving
            element.Rigidbody.velocity = XRInputController.Velocity(check);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        public void SetInteractionRange(float range)
        {
            range = Mathf.Clamp(value: range, min: Minimum, max: Maximum);
            interactionRange = range;
        }
        /// <summary>
        /// 
        /// </summary>
        public float GetInteractionRange()
        {
            return interactionRange;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetInteractionValue(float value)
        {
            value = Mathf.Clamp(value, min: 0f, max: 1f);
            interactionRange = Mathf.Lerp(Minimum, Maximum, value);
        }
        /// <summary>
        /// Returns true when either hand is currently manipulating something
        /// </summary>
        /// <returns></returns>
        public bool Manipulating()
        {
            return dominantManipulation.manipulating || nonDominantManipulation.manipulating;
        }
        /// <summary>
        /// 
        /// </summary>
        private static void CancelManipulation(Check check, ManipulationInformation information)
        {
            foreach (XRManipulatableElement element in information.manipulatedElements)
            {
                StopManipulating(element, check);
                element.CancelManipulation();
            }
            information.manipulatedElements.Clear();
            information.manipulating = false;
            information.closestXRManipulatableElement = null;
        }
    }
}
