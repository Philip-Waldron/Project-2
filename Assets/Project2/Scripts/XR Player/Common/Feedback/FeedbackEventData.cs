using System.Collections.Generic;
using UnityEngine;

namespace XR_Prototyping.Scripts.Common.Feedback
{
    [CreateAssetMenu(fileName = "FeedbackEventData", menuName = "XR Prototyping/Feedback", order = 1)]
    public class FeedbackEventData : ScriptableObject
    {
        public enum HapticFeedbackType
        {
            Impulse,
            Curve,
            Continuous,
            Byte
        }
        [Header("Audio Feedback")]
        public AudioClip audioClip;
        [Header("Haptic Feedback")]
        public HapticFeedbackType hapticFeedbackType;
        [Range(0, 1)] public float duration;
        [Range(0, 1)] public float amplitude;
        public AnimationCurve hapticCurve;
        public uint ByteBufferLength()
        {
            return (uint)(duration * 120);
        }
        public byte MaximumByteValue()
        {
            return (byte)(Mathf.Lerp(a: Minimum, b: Maximum, t: amplitude));
        }
        private const float Minimum = 0f, Maximum = 255f;
    }
}
