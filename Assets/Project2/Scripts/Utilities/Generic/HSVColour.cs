using System;
using UnityEngine;
using UnityEngine.Events;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace XR_Prototyping.Scripts.Utilities.Generic
{
    public class HSVColour : MonoBehaviour
    {
        public Color colour = new Color(0,0,0,1);
        [Space(5)] public ColourChangeEvent onColourChanged;
        [Serializable] public class ColourChangeEvent : UnityEvent<Color> {}
        
        private HSV hsv;

        [Serializable] public struct HSV
        {
            [Range(0f, 1f)] public float h, s, v;

            public HSV (float hValue, float sValue, float vValue)
            {
                h = hValue;
                s = sValue;
                v = vValue;
            }
        }

        private void Awake()
        {
            SetHSV(new HSV(colour.r, colour.g, colour.r));
        }

        private void Update()
        {
            colour = Color.HSVToRGB(hsv.h, hsv.s, hsv.v);
            onColourChanged.Invoke(colour);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hue"></param>
        public void SetHue(float hue)
        {
            hsv.h = hue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saturation"></param>
        public void SetSaturation(float saturation)
        {
            hsv.s = saturation;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(float value)
        {
            hsv.v = value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="setHSV"></param>
        public void SetHSV(HSV setHSV)
        {
            SetHue(setHSV.h);
            SetSaturation(setHSV.s);
            SetValue(setHSV.v);
        }
    }
}