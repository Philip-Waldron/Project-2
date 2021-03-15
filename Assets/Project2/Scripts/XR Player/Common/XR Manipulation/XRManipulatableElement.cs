using System;
using System.Collections.Generic;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Manipulation;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Manipulation.Snapping;
using XR_Prototyping.Scripts.Utilities.Extensions;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Manipulation
{
    public class XRManipulatableElement : XRInputAbstraction
    {
        [Serializable] public struct ManipulationCache
        {
            public Vector3 position, rotation, scale, localPosition;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="transform"></param>
            public void SetState(Transform transform)
            {
                position = transform.position;
                localPosition = transform.localPosition;
                rotation = transform.eulerAngles;
                scale = transform.localScale;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="transform"></param>
            public void ResetState(Transform transform)
            {
                transform.position = position;
                transform.eulerAngles = rotation;
                transform.localScale = scale;
            }
        }
        //--------------------------------------------------------------------------------------------------------------
        [Header("XR Manipulatable Element Behaviour")]
        [SerializeField] public ManipulationBehaviour manipulationBehaviour = ManipulationBehaviour.Manipulatable;
        [SerializeField] public SnappingBehaviour snappingBehaviour = SnappingBehaviour.SnappingDisabled;
        [SerializeField] public ScalingBehaviour scalingBehaviour = ScalingBehaviour.ScalingEnabled;
        [SerializeField, Range(1f, 0f), Space(height: 10)] private float damping = .75f;
        [SerializeField, Range(0f, 100f)] private float mass = 1f, drag = 10f;
        //--------------------------------------------------------------------------------------------------------------
        public enum ManipulationBehaviour { Manipulatable, NonManipulatable }
        public enum SnappingBehaviour { SnappingEnabled, SnappingDisabled }
        public enum ScalingBehaviour { ScalingEnabled, ScalingDisabled }
        //--------------------------------------------------------------------------------------------------------------
        private Vector3 initialScale;
        private Transform manipulationProxy, snapProxy, defaultParent;
        private ManipulationCache initialState;
        protected SnapVisual snapVisual;
        //--------------------------------------------------------------------------------------------------------------
        public Rigidbody Rigidbody { get; private set; }
        public bool Manipulated { get; private set; }
        public bool GroupParent { get; set; }
        public bool IsInGroup { get; private set; }
        private bool ManipulationAllowed => manipulationBehaviour == ManipulationBehaviour.Manipulatable;
        private bool SnappingAllowed => snappingBehaviour == SnappingBehaviour.SnappingEnabled;
        private bool ScalingAllowed => scalingBehaviour == ScalingBehaviour.ScalingEnabled;
        public XRManipulatableElement ManipulatableGroupParent { get; private set; }
        //--------------------------------------------------------------------------------------------------------------
        private Vector3 SetScale => manipulationProxy.lossyScale;
        protected float ScaleFactor => SetScale.magnitude / initialScale.magnitude;
        //--------------------------------------------------------------------------------------------------------------
        [HideInInspector] public List<XRManipulatableElement> groupChildren = new List<XRManipulatableElement>();
        //--------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            // Call the generic override method
            ElementAwake();
            // Cache references
            Transform elementTransform = transform;
            defaultParent = elementTransform.parent;
            initialScale = elementTransform.lossyScale;
            Vector3 position = elementTransform.position;
            // Create required elements
            Rigidbody = gameObject.TryGetComponent(out Rigidbody rigidBody) ? rigidBody : gameObject.AddConfiguredRigidbody(mass, drag);
            // Create the various proxies used by the element
            snapProxy = Set.Object(defaultParent == null ? null : defaultParent.gameObject, $"{name} Snapping Proxy", position, local: false).transform;
            manipulationProxy = Set.Object(gameObject, $"{name} Manipulation Proxy", Vector3.zero, local: false).transform;
            // Create a reference to the snap visual
            snapVisual = gameObject.AddComponent<SnapVisual>();
            snapVisual.CreateSnapVisual(name);
        }
        private void Start() => ElementStart();
        private void Update()
        {
            // Call the generic override method
            ElementUpdate();
            // Determine how the object should be moving
            ElementBehaviour();
            // Determine whether the object should be snapping or not
            CheckSnapState();
            // Visualise the manipulatable object debug information
            if (!XRDebug.Enabled) return;
            DebugVisual();
        }
        //--------------------------------------------------------------------------------------------------------------
        protected virtual void ElementAwake() {}
        protected virtual void ElementStart() {}
        protected virtual void ElementUpdate() {}
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        private void ElementBehaviour()
        {
            if (!Manipulated) return;
            transform.LerpTransforms(snapped ? snapProxy : manipulationProxy, damping);
            if (ScalingAllowed)
            {
                transform.LerpScale(SetScale, damping);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void CheckSnapState()
        {
            if (SnappingAllowed && !Manipulated && snapVisual != null)
            {
                snapVisual.SetSnapVisualState(state: false, from: Vector3.zero, to: Vector3.zero, snapDistance: float.Epsilon);
            }
            if (!SnappingAllowed || !Manipulated) return;
            // Cache some references
            Vector3 cachePosition = manipulationProxy.position;
            // Check what colliders this object is overlapping
            foreach (Collider overlap in Physics.OverlapSphere(cachePosition, XRManipulationController.SnapRadius))
            {
                // Find if there are any surfaces that can be snapped to within range of the object  
                bool validSurfaceInRange = overlap.TryGetComponent(out XRSnapSurface snapSurface);
                // Set the snap visual information
                if (validSurfaceInRange)
                {
                    bool shouldObjectSnap = snapSurface.ShouldSnap(cachePosition, out Vector3 snapLocation, out float duration, out float distance);
                    // If the object should snap, make the snap proxy move to the defined snap location
                    if (shouldObjectSnap)
                    {
                        snapProxy.position = snapLocation;
                        snapProxy.rotation = snapSurface.transform.rotation;
                        snapProxy.localScale = manipulationProxy.localScale;
                    }
                    // Otherwise align it with the manipulation proxy
                    else
                    {
                        snapProxy.Transforms(manipulationProxy, scale: true);
                    }
                    // The object should snap, but hasn't
                    if (!snapped && shouldObjectSnap)
                    {
                        snapped = true;
                    }
                    // The object is snapped and shouldn't be
                    else if (snapped && !shouldObjectSnap)
                    {
                        snapped = false;
                    }
                    // Set the snap visual state
                    snapVisual.SetSnapVisualState(state: true, from: transform.position, to: snapLocation, distance);
                    return;
                }
                // Set the snap visual state when there is nothing to snap to
                snapVisual.SetSnapVisualState(state: false, from: Vector3.zero, to: Vector3.zero, float.Epsilon);
            }
        }
        private float cacheDuration;
        private bool snapped;
        /// <summary>
        /// 
        /// </summary>
        private void DebugVisual()
        {
            // Cache references
            Transform element = transform, proxy = manipulationProxy;
            Vector3 elementPosition = element.position, proxyPosition = proxy.position;
            // Draw debug visualisation
            XRDebug.DrawLine($"{GetInstanceID()}, Manipulation Connection", elementPosition, proxyPosition, Color.magenta);
            XRDebug.DrawSphere($"{GetInstanceID()}, Manipulation Element", elementPosition, .02f, Color.red);
            XRDebug.DrawSphere($"{GetInstanceID()}, Manipulation Proxy", proxyPosition, .025f, Color.magenta);
        }
        /// <summary>
        /// 
        /// </summary>
        public void ManipulateStart(Transform manipulator)
        {
            ElementManipulateStart();
            // Now the object is being manipulated
            Manipulated = true;
            // Snap the manipulation proxy to the current object transform
            manipulationProxy.Transforms(transform, scale: true);
            // Cache the position of the object
            initialState.SetState(manipulationProxy);
            // Set the manipulation proxy to be a child of the supplied object
            SetManipulationParent(manipulator);
        }
        /// <summary>
        /// This will determine what is manipulating the object
        /// </summary>
        public void SetManipulationParent(Transform manipulator)
        {
            manipulationProxy.transform.SetParent(manipulator);
        }
        /// <summary>
        /// 
        /// </summary>
        public void ManipulateStay() { ElementManipulateStay();}
        /// <summary>
        /// 
        /// </summary>
        public void ManipulateEnd()
        {
            ElementManipulateEnd();
            // The object is no longer being manipulated
            Manipulated = false;
            // Reset the parentage of the manipulation proxy, but only if not in a group
            if (IsInGroup) return;
            SetManipulationParent(defaultParent);
        }
        //--------------------------------------------------------------------------------------------------------------
        protected virtual void ElementManipulateStart() {}
        protected virtual void ElementManipulateStay() {}
        protected virtual void ElementManipulateEnd() {}
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public void CancelManipulation()
        {
            initialState.ResetState(manipulationProxy);
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddToGroup(XRManipulatableElement parent)
        {
            IsInGroup = true;
            ManipulatableGroupParent = parent;
            transform.SetParent(ManipulatableGroupParent.transform);
        }
        /// <summary>
        /// 
        /// </summary>
        public void RemoveFromGroup()
        {
            IsInGroup = false;
            ManipulatableGroupParent = null;
            transform.SetParent(defaultParent);
        }
    }
}
