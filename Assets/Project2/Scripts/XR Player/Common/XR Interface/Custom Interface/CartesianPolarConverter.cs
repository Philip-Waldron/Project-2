using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Custom_Interface
{
    [RequireComponent(typeof(RectTransform))]
    public class CartesianPolarConverter : XRInputAbstraction
    {
        [Header("Polar Coordinate Settings")] 
        [SerializeField] private bool lockedRange;
        [SerializeField, Range(0f, .1f)] private float snapDistance = .025f;
        [Serializable] public struct PolarCoordinates
        {
            public float r, θ;
            public PolarCoordinates (float rValue, float θValue)
            {
                r = rValue;
                θ = θValue;
            }
        }
        public enum InteractionMethod
        {
            Indirect,
            Dislocated
        }

        private bool beingSet = true;
        private Transform proxy, origin, coordinate, coordinateProxy, construction;
        private PolarCoordinates setPolarCoordinates;
        
        private RectTransform RectTransform => GetComponent<RectTransform>();

        #region Dynamic Values
        
        private float MaximumRange => RectTransform.sizeDelta.x * .5f;
        private float MinimumRange => lockedRange ? MaximumRange : 0f;
        private Vector3 NonNormalisedPosition => proxy.transform.localPosition; 
        private Vector3 NormalisedPosition => new Vector3(NonNormalisedPosition.x, NonNormalisedPosition.y, 0f); 
        private Vector3 OriginForward => proxy.transform.position - origin.transform.position;
        private Vector3 OriginRotation => new Vector3(Mathf.Lerp(360f, 0f, setPolarCoordinates.θ), 90f, 0f);
        private float CoordinateValue => Mathf.Clamp(Vector3.Distance(origin.position, proxy.position), MinimumRange, MaximumRange);
        private Vector3 CoordinateLocalPosition => new Vector3(0f, 0f, beingSet ? -Mathf.Lerp(MinimumRange, MaximumRange, setPolarCoordinates.r) : CoordinateValue);
        
        public Vector2 CartesianCoordinate => new Vector2(
            x: Calculations.NormalisedCartesianCoordinate(coordinateProxy.localPosition.x, MaximumRange), 
            y: Calculations.NormalisedCartesianCoordinate(coordinateProxy.localPosition.y, MaximumRange));
        public PolarCoordinates PolarCoordinate => new PolarCoordinates( 
            rValue: Calculations.CartesianToPolar(CartesianCoordinate).r, 
            θValue: Calculations.NormaliseRadianPolarCoordinate(Calculations.CartesianToPolar(CartesianCoordinate).θ));

        #endregion

        private void Awake()
        {
            construction = Set.Object(gameObject, "[Polar Construction]", Vector3.zero).transform;
            proxy = Set.Object(construction.gameObject, "[Polar Construction] Proxy", Vector3.zero).transform;
            origin = Set.Object(construction.gameObject, "[Polar Construction] Origin", Vector3.zero).transform;
            coordinateProxy = Set.Object(construction.gameObject, "[Polar Construction] Coordinate Proxy", Vector3.zero).transform;
            coordinate = Set.Object(origin.gameObject, "[Polar Construction] Coordinate", Vector3.zero).transform;
        }

        private void Update()
        {
            AlignConstruction();
            switch (beingSet)
            {
                case true:
                    HaveValuesSet();
                    break;
                case false:
                    SetValues();
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void AlignConstruction()
        {
            construction.ResetLocalTransform();
        }
        /// <summary>
        /// External values setting the state
        /// </summary>
        private void HaveValuesSet()
        {
            origin.localEulerAngles = OriginRotation;
            coordinate.localPosition = CoordinateLocalPosition;
            proxy.position = coordinate.position;
            coordinateProxy.Transforms(coordinate);
        }
        /// <summary>
        /// Proxy is determining the coordinate values
        /// </summary>
        private void SetValues()
        {
            proxy.localPosition = Vector3.Distance(NormalisedPosition, origin.localPosition) >= snapDistance ? NormalisedPosition : Vector3.zero;
            origin.forward = OriginForward;
            coordinate.localPosition = CoordinateLocalPosition;
            coordinateProxy.Transforms(coordinate);
        }
        /// <summary>
        /// This is basic debug information
        /// </summary>
        private void LateUpdate()
        {
            Vector3 position = coordinateProxy.position;
            XRDebug.DrawLine($"{GetInstanceID()} Polar Coordinate Connection", origin.position, position, Color.black);
            XRDebug.DrawRay($"{GetInstanceID()} Polar Coordinate", position, -transform.forward, .025f, Color.black);
            XRDebug.Log($"{GetInstanceID()} Coordinate Log", $"(x:{Math.Round(CartesianCoordinate.x, 2)}, y:{Math.Round(CartesianCoordinate.y, 2)})  (r:{Math.Round(PolarCoordinate.r, 2)}, θ:{Math.Round(PolarCoordinate.θ, 2)})", position, .05f);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SetProxyPosition(Vector3 position)
        {
            beingSet = false;
            proxy.position = position;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polarCoordinates"></param>
        public void SetPolarCoordinates(PolarCoordinates polarCoordinates)
        {
            beingSet = true;
            setPolarCoordinates = polarCoordinates;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 CoordinatePosition()
        {
            return coordinateProxy.position;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetClampedState(bool state)
        {
            lockedRange = state;
        }
    }
}
