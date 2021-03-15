using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VR_Prototyping.Plugins.Demigiant.DOTween.Modules;
using XR_Prototyping.Scripts.Common.Feedback;

namespace XR_Prototyping.Scripts.Common.XR_Interface
{
    public class XRStateTransitionElement : MonoBehaviour
    {
        [Serializable] public class StateTransitionElements
        {
            [Serializable] public struct OffsetStates
            {
                [Serializable] public struct Offset
                {
                    public string stateName;
                    [Range (-.1f, .1f)] public float offset;
                    [Range (0f, 1f)] public float duration;
                }
                
                public Transform offset;
                public List<Offset> offsets;
            }
            [Serializable] public struct TintStates
            {
                [Serializable] public struct Tint
                {
                    public string stateName;
                    public Color color;
                    [Range (0f, 1f)] public float duration;
                }
                public Image tint;
                public List<Tint> tints;
            }

            public List<OffsetStates> offsets = new List<OffsetStates>();
            public List<TintStates> tints = new List<TintStates>();

            // public XRFeedbackController.FeedbackEvent feedbackEvent;

            public void SetVisualState(StateTransitionElements stateTransitionElements, bool immediate)
            {
                if (immediate)
                {
                    ImmediatelySetVisualState(stateTransitionElements);
                }
                else
                {
                    SetVisualState(stateTransitionElements);
                }
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="stateTransitionElements"></param>
            private void ImmediatelySetVisualState(StateTransitionElements stateTransitionElements)
            {
                /*
                foreach (Offset offset in stateTransitionElements.offsets)
                {
                    Vector3 offsetLocalPosition = offset.transform.localPosition;
                    offset.transform.localPosition = new Vector3(offsetLocalPosition.x, offsetLocalPosition.y, - offset.offset);
                }
                foreach (Tint tint in stateTransitionElements.tints)
                {
                    tint.tint.color = tint.color;
                }
                */
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="stateTransitionElements"></param>
            /// <param name="duration"></param>
            private void SetVisualState(StateTransitionElements stateTransitionElements)
            {
                /*
                foreach (Offset offset in stateTransitionElements.offsets)
                {
                    offset.transform.DOLocalMoveZ(
                        endValue: -offset.offset,
                        duration: offset.duration);
                }
                foreach (Tint tint in stateTransitionElements.tints)
                {
                    tint.tint.DOColor(
                        endValue: tint.color,
                        duration: tint.duration);
                }
                */
            }
        }
    }
}