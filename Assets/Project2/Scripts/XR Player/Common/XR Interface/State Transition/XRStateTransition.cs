using UnityEngine;
using UnityEngine.Events;
using XR_Prototyping.Scripts.Common.Feedback;

namespace XR_Prototyping.Scripts.Common.XR_Interface.State_Transition
{
    [CreateAssetMenu(fileName = "StateTransition", menuName = "XR Prototyping/State Transition", order = 1)]
    public class XRStateTransition : ScriptableObject
    {
        [Header("State Colour")]
        public Color stateColour = new Color(0,0,0,1);
        [Header("State Offset Settings")]
        [Range(-1f, 1f)] public float stateOffsetAmount = 0f;
        public bool enableOffset;
        [Header("State Transition Duration")]
        [Range(.1f, 1f)] public float stateTransitionDuration = .1f;
        [Header("State Transition Feedback Data")]
        public XRFeedbackController.FeedbackEvent stateTransitionFeedbackEvent;
    }
}
