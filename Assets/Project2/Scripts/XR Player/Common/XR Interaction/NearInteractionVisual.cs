using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Interaction
{
    public class NearInteractionVisual : XRInputAbstraction
    {
        [Header("Settings")] 
        [SerializeField, Range(0f, 1f)] private float minimumSize = .01f;
        [SerializeField, Range(0f, 1f)] private float maximumSize = 1f;
        [Header("References")]
        [SerializeField] private Transform dynamicVisual;
        [SerializeField] private List<Image> visualElements = new List<Image>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="information"></param>
        public void SetVisual(IndirectInteraction.InteractionInformation information)
        {
            Transform visualTransform = transform;
            visualTransform.position = information.hit.point;
            visualTransform.forward = information.hit.normal;
            dynamicVisual.ScaleFactor(Mathf.Clamp(XRInteractionController.NearDistanceScale(information.interactionDistance), minimumSize, maximumSize));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetVisualState(bool state)
        {
            foreach (Image image in visualElements)
            {
                image.enabled = state;
            }
        }
    }
}
