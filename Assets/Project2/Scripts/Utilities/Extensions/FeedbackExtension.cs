using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR;
using XR_Prototyping.Scripts.Common;
using XR_Prototyping.Scripts.Common.Feedback;

namespace XR_Prototyping.Scripts.Utilities
{
    public static class FeedbackExtension
    {
        private const uint Channel = 0;
        public static void HapticEvent(this InputDevice inputDevice, FeedbackEventData data)
        {
            if (data == null) return;
            switch (data.hapticFeedbackType)
            {
                case FeedbackEventData.HapticFeedbackType.Impulse:
                    inputDevice.SendHapticImpulse(
                        Channel, 
                        data.amplitude, 
                        data.duration);
                    break;
                case FeedbackEventData.HapticFeedbackType.Curve:
                    inputDevice.SendHapticBuffer(Channel, AnimationCurveToByteBuffer(data.MaximumByteValue(), data.hapticCurve, data.ByteBufferLength()));
                    break;
                case FeedbackEventData.HapticFeedbackType.Continuous:
                    // Todo, figure out how to do this properly
                    inputDevice.SendHapticBuffer(Channel, AnimationCurveToByteBuffer(data.MaximumByteValue(), data.hapticCurve, data.ByteBufferLength()));
                    break;
                case FeedbackEventData.HapticFeedbackType.Byte:
                    List<byte> bytes = new List<byte>();
                    for (int i = 0; i < data.ByteBufferLength(); i++)
                    {
                        bytes.Add(item: data.MaximumByteValue());
                    }
                    // Debug.Log(message: $"{data.ByteBufferLength()} @ {data.MaximumByteValue()}");
                    inputDevice.SendHapticBuffer(Channel, buffer: bytes.ToArray());
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// Takes in an animation curve an outputs a normalised byte buffer that can be used for haptics
        /// </summary>
        private static byte[] AnimationCurveToByteBuffer(float maxByte, AnimationCurve animationCurve, uint bufferLength)
        {
            float animationMax = float.NegativeInfinity;
            for (int i = 0; i < animationCurve.length; i++)
            {
                float value = animationCurve[i].value;
                if (value > animationMax)
                {
                    animationMax = value;
                }
            }
            float scalar =  maxByte / animationMax;
            List<byte> byteBuffer = new List<byte>();
            float segments = 1f / bufferLength;
            float time = 0;
            for (int i = 1; i < bufferLength; i++)
            {
                time += segments;
                float value = animationCurve.Evaluate(time: time);
                int normalised = (int)(value * scalar);
                // Debug.Log(message: $"{value} → <b>{normalised}</b> @ {time} = {1} / {bufferLength} (<i>{segments}</i>)");
                byteBuffer.Add((byte)normalised);
            }
            return byteBuffer.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        public static void AudioFeedback(this AudioSource source, FeedbackEventData data)
        {
            if (data.audioClip == null) return;
            source.clip = data.audioClip;
            source.Play();
        }
    }
}
