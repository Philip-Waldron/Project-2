using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.State_Transition;
using XR_Prototyping.Scripts.Utilities.Generic;
using Slider = XR_Prototyping.Scripts.Common.XR_Interface.Unity_Interface_Abstractions.Slider;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Custom_Interface
{
    [RequireComponent(typeof(HSVColour))]
    public class HSVColourPicker : Slider
    {
        private enum HSVColourPickerType
        {
            Simple,
            Advanced,
            ValueOnly
        }

        [Header("HSV Colour Picker Settings")] 
        [SerializeField] private SliderType colourPickerInteractionType = SliderType.Normal;
        [SerializeField] private XRInputController.Event triggerEvent = XRInputController.Event.GripPress;
        [SerializeField] private HSVColourPickerType colourPickerType = HSVColourPickerType.Simple;
        [SerializeField] private CartesianPolarConverter.InteractionMethod interactionMethod = CartesianPolarConverter.InteractionMethod.Indirect;
        [Header("HSV Colour Picker References")]
        [SerializeField] private CartesianPolarConverter cartesianPolarConverter;
        [SerializeField] private Transform colourVisual;
        [SerializeField] private Image valueBlock;
        [SerializeField] private Slider hueSlider, saturationSlider, valueSlider;
        [Header("Colour Data")] 
        public Color colour;
        public HSVColour.HSV colourData;

        private bool SettingVolumetricColour { get; set; }
        private bool IndirectSetting { get; set; }
        private XRInputController.Check latchedHand;

        private Color Colour => HSVColour.colour;
        private HSVColour HSVColour => GetComponent<HSVColour>();
        private bool SetColourInformation => SettingVolumetricColour || IndirectSetting;
        private HSVColour.HSV HSV => 
            SetColourInformation
                // Set the values according to the spatial coordinates 
                ? new HSVColour.HSV(
                    hValue: cartesianPolarConverter.PolarCoordinate.θ,
                    sValue: cartesianPolarConverter.PolarCoordinate.r,
                    vValue: UnitySlider.value)
                // Set the values according to the advanced sliders
                : new HSVColour.HSV(
                    hValue: hueSlider.UnitySlider.value,
                    sValue: saturationSlider.UnitySlider.value,
                    vValue: valueSlider.UnitySlider.value);

        private Color ValueColour => new Color(0, 0, 0, Mathf.Lerp(1, 0, HSV.v));

        private void LateUpdate()
        {
            SettingVolumetricColour = Enabled && SettingVolumetricColour;
            SetAdvancedSlidersState();
            
            if (!Enabled) return;
            
            // Set public colour picker data
            colour = Colour;
            colourData = HSV;
            // Communicate that colour information
            HSVColour.SetHSV(HSV);
            // Set visual states for that colour data
            colourVisual.position = cartesianPolarConverter.CoordinatePosition();
            valueBlock.color = ValueColour;
            // You can ignore this if it's a gestural colour picker
            if (colourPickerInteractionType == SliderType.Gestural) return;
            // If you are latched then you are setting the values directly, with the cached check data
            // TEST
            if (IndirectSetting) return;
            if (SettingVolumetricColour)
            {
                SetColourValues(latchedHand, direct: true);
                // Once you release the latch then you stop setting the colour data proactively 
                if (LatchEnd(latchedHand))
                {
                    // XRInteractionController.SetAllowedState(latchedHand, true);
                    SettingVolumetricColour = false;
                }
            }
            // If you are not setting the colour information actively, then you set the colour data from the HSV sliders
            else
            {
                SetPolarCoordinates();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetPolarCoordinates()
        {
            cartesianPolarConverter.SetPolarCoordinates(new CartesianPolarConverter.PolarCoordinates(rValue: saturationSlider.UnitySlider.value, θValue: hueSlider.UnitySlider.value));
            UnitySlider.value = HSV.v;
        }

        public override void EngageStart(XRInputController.Check check, bool immediate = false, bool direct = false) { }

        public override void EngageStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            // todo add this logic to a layer below this - in the abstraction layer
            if (IgnoreInteraction(check)) return;
            // Only register this data if it is a gestural slider
            if (colourPickerInteractionType != SliderType.Gestural) return;
            SettingVolumetricColour = true;
            SetColourValues(check, direct: false);
        }
        public override void EngageEnd(XRInputController.Check check, bool immediate = false, bool direct = false) { }

        public override void SelectStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            IndirectSetting = !direct;
        }

        public override void SelectStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            // Only set these values if it is a normal slider, either directly or indirectly
            if (colourPickerInteractionType != SliderType.Normal) return;
            // If this is not direct it means that you're indirectly setting the colour value
            if (!direct && IndirectSetting)
            {
                SetColourValues(check, direct: false);
            }
            // If you press down the defined button, you latch the interaction and this logic is then handled in LateUpdate()
            else if (direct || LatchStart(check))
            {
                // XRInteractionController.SetAllowedState(check, false);
                latchedHand = check;
                SettingVolumetricColour = true;
            }
        }
        public override void SelectEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            IndirectSetting = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool LatchStart(XRInputController.Check check)
        {
            return XRInputController.InputEvent(triggerEvent).State(check, XRInputController.InputEvents.InputEvent.Transition.Down);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool LatchEnd(XRInputController.Check check)
        {
            return XRInputController.InputEvent(triggerEvent).State(check, XRInputController.InputEvents.InputEvent.Transition.Up);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="direct"></param>
        private void SetColourValues(XRInputController.Check check, bool direct)
        {
            // Sets the slider value - this functions as the depth data of the colour picker
            // Don't do this if you're indirectly interacting however, or it will snap to the highest value
            if (!IndirectSetting)
            {
                ApplySliderValue();
            }
            // Depending on whether this is direct or not it sets the proxy at the collision or the raycast point
            cartesianPolarConverter.SetProxyPosition(ProxyPosition(check, direct));
            // Set the value of the advanced sliders
            hueSlider.UnitySlider.value = HSV.h;
            saturationSlider.UnitySlider.value = HSV.s;
            valueSlider.UnitySlider.value = HSV.v;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 ProxyPosition(XRInputController.Check check, bool direct)
        {
            if (direct)
            {
                return XRInputController.Position(check);
            }
            switch (interactionMethod)
            {
                case CartesianPolarConverter.InteractionMethod.Indirect:
                    return XRInteractionController.GetInteractionInformation(check).hit.point;
                case CartesianPolarConverter.InteractionMethod.Dislocated:
                    return Collider.ClosestPoint(XRInputController.Position(check));
                default:
                    return Vector3.zero;
            }
        }
        /// <summary>
        /// Set the visual state of the advanced sliders
        /// </summary>
        /// <param name="state"></param>
        private void OverrideAdvancedSlidersState(bool state)
        {
            hueSlider.SetState(state);
            saturationSlider.SetState(state);
            valueSlider.SetState(state);
        }
        /// <summary>
        /// Set the visual state of the advanced sliders
        /// </summary>
        private void SetAdvancedSlidersState()
        {
            switch (colourPickerType)
            {
                case HSVColourPickerType.Simple:
                    OverrideAdvancedSlidersState(false);
                    break;
                case HSVColourPickerType.Advanced:
                    OverrideAdvancedSlidersState(true);
                    break;
                case HSVColourPickerType.ValueOnly:
                    hueSlider.SetState(false);
                    saturationSlider.SetState(false);
                    valueSlider.SetState(true);
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(bool state)
        {
            base.SetState(state);
            OverrideAdvancedSlidersState(false);
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetColour(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            hueSlider.UnitySlider.value = h;
            saturationSlider.UnitySlider.value = s;
            valueSlider.UnitySlider.value = v;
        }
    }
}