using System.Collections.Generic;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VR_Prototyping.Plugins.Demigiant.DOTween.Modules;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.State_Transition;
using XR_Prototyping.Scripts.Utilities.Extensions;

namespace XR_Prototyping.Scripts.Common.XR_Interface
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class XRInterfaceAbstraction : XRInputAbstraction
    {
        protected enum EngagementState
        {
            None,
            Single,
            Double
        }
        protected enum InterfaceType
        {
            Worldspace,
            Anchored
        }

        protected enum InterfaceState
        {
            Engaged,
            Selected,
            Disengaged
        }

        [Header("XR Interface Settings")]
        [SerializeField] protected InterfaceType interfaceType = InterfaceType.Worldspace;
        [SerializeField] protected string labelText;
        
        [Header("XR Interface References")]
        [SerializeField] protected Image tintBacking;
        [SerializeField] protected Transform offsetElement;
        [SerializeField] protected TextMeshProUGUI label;
        [SerializeField] protected List<Image> interfaceImages = new List<Image>();
        
        [Header("XR Interface State Transitions")]
        [SerializeField] protected XRStateTransition normal;
        [SerializeField] protected XRStateTransition engaged;
        [SerializeField] protected XRStateTransition active;

        protected EngagementState engagementState = EngagementState.None;
        protected InterfaceState interfaceState = InterfaceState.Disengaged;
        
        public BoxCollider Collider => GetComponent<BoxCollider>();
        public Rigidbody Rigidbody => GetComponent<Rigidbody>();
        
        protected XRInputController.Check activeHand;

        public bool Enabled { get; set; } = true;

        private void Awake()
        {
            label.SetText(labelText);
            gameObject.AddConfiguredBoxCollider(trigger: true);
            gameObject.AddConfiguredRigidbody();
            XRInterfaceAwake();
        }
        private void Start()
        {
            XRInterfaceStart();
        }
        private void Update()
        {
            XRInterfaceUpdate();
        }
        /// <summary>
        /// 
        /// </summary>
        protected abstract void XRInterfaceAwake();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void XRInterfaceStart();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void XRInterfaceUpdate();
        /// <summary>
        /// 
        /// </summary>
        public abstract void EngageStart(XRInputController.Check check, bool immediate = false, bool direct = false);
        /// <summary>
        /// 
        /// </summary>
        public abstract void EngageStay(XRInputController.Check check, bool immediate = false, bool direct = false);
        /// <summary>
        /// 
        /// </summary>
        public abstract void EngageEnd(XRInputController.Check check, bool immediate = false, bool direct = false);
        /// <summary>
        /// 
        /// </summary>
        public abstract void SelectStart(XRInputController.Check check, bool immediate = false, bool direct = false);
        /// <summary>
        /// 
        /// </summary>
        public abstract void SelectStay(XRInputController.Check check, bool immediate = false, bool direct = false);
        /// <summary>
        /// 
        /// </summary>
        public abstract void SelectEnd(XRInputController.Check check, bool immediate = false, bool direct = false);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="state"></param>
        /// <param name="immediate"></param>
        protected void Transition(XRStateTransition transition, InterfaceState state, bool immediate)
        {
            if (transition == null) return;
            if (immediate)
            {
                tintBacking.color = transition.stateColour;
                if (!transition.enableOffset) return;
                Vector3 position = offsetElement.localPosition;
                offsetElement.transform.localPosition = new Vector3(position.x, position.y, -transition.stateOffsetAmount);
            }
            else
            {
                tintBacking.DOColor(
                    endValue: transition.stateColour, 
                    duration: transition.stateTransitionDuration);
                if (!transition.enableOffset) return;
                offsetElement.DOLocalMoveZ(
                    endValue: -transition.stateOffsetAmount,
                    duration: transition.stateTransitionDuration);
            }
            interfaceState = state;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public abstract void SetState(bool state);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        protected bool IgnoreInteraction(XRInputController.Check check)
        {
            return interfaceType == InterfaceType.Anchored && check == XRInputController.NonDominantHand();
        }
    }
}
