using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.Feedback;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VR_Prototyping.Plugins.Demigiant.DOTween.Modules;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.State_Transition;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Unity_Interface_Abstractions
{
    [RequireComponent(typeof(UnityEngine.UI.Slider))]
    public class Slider : XRInterfaceAbstraction
    {
        protected enum SliderType
        {
            Normal,
            Gestural
        }
        
        [Header("Slider Settings")]
        [SerializeField] private TextMeshProUGUI sliderLabel;
        [SerializeField] private Transform minimum, maximum;
        [SerializeField] private Vector2 sliderLabelRange = new Vector2(0,1);
        [SerializeField] protected SliderType sliderType = SliderType.Normal;

        private GameObject proxy;
        
        public UnityEngine.UI.Slider UnitySlider => GetComponent<UnityEngine.UI.Slider>();
        private float Minimum => minimum.localPosition.x;
        private float Maximum => maximum.localPosition.x;
        private float Current => proxy.transform.localPosition.x;

        protected override void XRInterfaceAwake()
        {
            proxy = Set.Object(minimum.parent.gameObject, "[Proxy]", minimum.localPosition);
        }

        protected override void XRInterfaceStart()
        {

        }

        protected override void XRInterfaceUpdate()
        {
            SetSliderProxyPosition();
            SliderDebug();
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void SetSliderProxyPosition()
        {
            proxy.transform.position = ClosestPoint(check: XRInputController.DominantHand());
            proxy.transform.localPosition = new Vector3(
                Mathf.Clamp(
                    proxy.transform.localPosition.x, 
                    minimum.localPosition.x, 
                    maximum.localPosition.x),
                0f, 
                0f);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SliderDebug()
        {
            XRDebug.DrawLine($"{gameObject.GetInstanceID()} Range", minimum.position, maximum.position, Enabled ? Color.black : Color.clear);
            XRDebug.DrawRay($"{gameObject.GetInstanceID()} Proxy", proxy.transform.position, -transform.forward, .01f, Enabled ? Color.black : Color.clear);
        }

        public override void EngageStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(engaged, InterfaceState.Engaged, immediate);
        }

        public override void EngageStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            if (sliderType == SliderType.Gestural)
            {
                ApplySliderValue();
            }
        }

        public override void EngageEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(normal, InterfaceState.Disengaged, immediate);
        }

        public override void SelectStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackStart);
            Transition(active, InterfaceState.Selected, immediate);
        }

        public override void SelectStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            ApplySliderValue();
        }

        public override void SelectEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            XRFeedbackController.Feedback(check, active.stateTransitionFeedbackEvent.feedbackEnd);
            Transition(direct ? normal : engaged, direct ? InterfaceState.Disengaged : InterfaceState.Engaged, immediate);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(bool state)
        {
            Enabled = state;
            label.SetText(state ? labelText : "");
            sliderLabel.SetText(state ? sliderLabel.text : "");
            foreach (Image image in interfaceImages)
            {
                image.enabled = state;
            }
            Collider.enabled = state;
        }
        /// <summary>
        /// 
        /// </summary>
        protected void ApplySliderValue()
        {
            UnitySlider.value = SliderValue();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float SliderValue()
        {
            sliderLabel.SetText($"{Math.Round(Mathf.Lerp(sliderLabelRange.x, sliderLabelRange.y, UnitySlider.value), 2)}");
            return Mathf.Lerp(UnitySlider.minValue, UnitySlider.maxValue, Mathf.InverseLerp(Minimum, Maximum, Current));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private Vector3 ClosestPoint(XRInputController.Check check)
        {
            switch (sliderType)
            {
                case SliderType.Normal:
                    return XRInteractionController.GetInteractionInformation(check).hit.point;
                case SliderType.Gestural:
                    return Collider.ClosestPoint(XRInputController.Position(check));
                default:
                    return Vector3.zero;
            }
        }
    }
}
