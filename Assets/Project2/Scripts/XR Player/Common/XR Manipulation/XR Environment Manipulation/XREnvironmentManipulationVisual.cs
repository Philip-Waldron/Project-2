using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Manipulation.XR_Environment_Manipulation;
using TMPro;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Manipulation.XR_Environment_Manipulation
{
    [RequireComponent(typeof(XREnvironmentManipulation))]
    public class XREnvironmentManipulationVisual : XRInputAbstraction
    {
        [Header("XR Environment Manipulation Visual Settings")] 
        [SerializeField, Range(0f, .1f)] private float offset = .05f;
        [SerializeField, Range(float.Epsilon, .0025f)] private float width = .001f;
        [Header("XR Environment Manipulation VisualReferences")] 
        [SerializeField] private GameObject scaleInformation;
        [SerializeField] private TextMeshProUGUI scaleReadout;
        [SerializeField] private Material scalingVisualMaterial;
        
        private LineRenderer line;
        private EnvironmentManipulationHand nonDominant, dominant;
        private Transform bimanualProxy;

        [Serializable] private struct EnvironmentManipulationHand
        {
            private Transform hand, offset;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public void InitialiseHand(GameObject parent)
            {
                hand = Set.Object(parent, "[Environment Manipulation Hand]", Vector3.zero).transform;
                offset = Set.Object(hand.gameObject, "[Environment Manipulation Hand] Offset", Vector3.zero).transform;
            }
            /// <summary>
            /// 
            /// </summary>
            public void SetInformation(Vector3 position, Vector3 lookAt, float depth)
            {
                hand.position = position;
                hand.LookAt(lookAt);
                offset.localPosition = Set.Offset(depth);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public Transform GetInformation()
            {
                return offset;
            }
        }
        
        private XREnvironmentManipulation XREnvironmentManipulation => GetComponent<XREnvironmentManipulation>();

        private enum VisualState
        {
            Disabled,
            DirectGrabbing,
            Listening,
            Scaling
        }

        private VisualState visualState;

        private void Awake()
        {
            GameObject visualParent = Set.Object(gameObject, "[Environment Manipulation Visual]", Vector3.zero);
            line = visualParent.Line(scalingVisualMaterial, width, startEnabled: false);
            dominant.InitialiseHand(visualParent);
            nonDominant.InitialiseHand(visualParent);
        }
        private void Update()
        {
            CheckVisualState();
            SetVisualInformation();
            SetVisualState();
        }
        /// <summary>
        /// 
        /// </summary>
        private void CheckVisualState()
        {
            if (XREnvironmentManipulation.BimanualListening)
            {
                visualState = VisualState.Listening;
            }
            else if (XREnvironmentManipulation.Bimanual)
            {
                visualState = VisualState.Scaling;
            }
            else if (XREnvironmentManipulation.Manipulating)
            {
                visualState = VisualState.DirectGrabbing;
            }
            else
            {
                visualState = VisualState.Disabled;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetVisualInformation()
        {
            dominant.SetInformation(XRInputController.Position(XRInputController.DominantHand()), XRInputController.Position(XRInputController.NonDominantHand()), offset);
            nonDominant.SetInformation(XRInputController.Position(XRInputController.NonDominantHand()), XRInputController.Position(XRInputController.DominantHand()), offset);
            line.DrawLine(dominant.GetInformation(), nonDominant.GetInformation());
            scaleInformation.transform.position = Set.MidpointPosition(dominant.GetInformation(), nonDominant.GetInformation());
            scaleInformation.transform.LookAwayFrom(XRInputController.Position(XRInputController.Check.Head), Vector3.up);
            scaleReadout.SetText(text: $"1:{Math.Round(XREnvironmentManipulation.ScaleFactor, 2)}");
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetVisualState()
        {
            switch (visualState)
            {
                case VisualState.Disabled:
                    line.enabled = false;
                    scaleInformation.SetActive(false);
                    break;
                case VisualState.DirectGrabbing:
                    line.enabled = false;
                    scaleInformation.SetActive(false);
                    break;
                case VisualState.Listening:
                    line.enabled = true;
                    scaleInformation.SetActive(false);
                    break;
                case VisualState.Scaling:
                    line.enabled = true;
                    scaleInformation.SetActive(true);
                    break;
                default:
                    return;
            }
        }
    }
}