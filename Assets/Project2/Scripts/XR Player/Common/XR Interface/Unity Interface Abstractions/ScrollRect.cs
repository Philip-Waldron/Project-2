using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace XR_Prototyping.Scripts.Common.XR_Interface.Unity_Interface_Abstractions
{
    [RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
    [RequireComponent(typeof(BoxCollider))]
    public class ScrollRect : XRInterfaceAbstraction
    {
        [Header("Scroll Rect Settings")]
        [SerializeField, Range(0f, 1f)] private float scrollSmoothing = .75f;
        [SerializeField, Range(0f, 10f)] private float scrollMultiplier, directScrollMultiplier = 1f, anchoredScrollMultiplier = .05f;
        [SerializeField] private ScrollDirection scrollDirection = ScrollDirection.Direct;
        [SerializeField] private ScrollType scrollType = ScrollType.Delta;

        private float value;
        private Vector3 startPosition, currentPosition, previousPosition;

        private enum ScrollType
        {
            Delta, 
            Hold
        }
        private enum ScrollDirection
        {
            Direct, 
            Inverted
        }

        private UnityEngine.UI.ScrollRect UnityScrollRect => GetComponent<UnityEngine.UI.ScrollRect>();

        private float DirectDelta()
        {
            currentPosition = XRInteractionController.GetInteractionInformation(activeHand).hit.point; 
            float delta = (currentPosition.y - previousPosition.y) * directScrollMultiplier;
            previousPosition = currentPosition;
            return delta;
        }

        protected override void XRInterfaceAwake() { }
        protected override void XRInterfaceStart() { }

        protected override void XRInterfaceUpdate()
        {
            if (interfaceType != InterfaceType.Anchored) return;
            if (XRInputController.AxisDirection(XRInputController.NonDominantHand(), XRInputController.Cardinal.Forward) || XRInputController.AxisDirection(XRInputController.NonDominantHand(), XRInputController.Cardinal.Back))
            {
                VerticalScrolling(ScrollDirection.Direct, XRInputController.AxisValue(XRInputController.NonDominantHand(), truncated: true).y * anchoredScrollMultiplier);
            }
        }

        public override void EngageStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(engaged, InterfaceState.Engaged, immediate);
        }

        public override void EngageStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            VerticalScrolling(scrollDirection, input: Value(check));
        }

        public override void EngageEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(normal, InterfaceState.Disengaged, immediate);
        }

        public override void SelectStart(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(active, InterfaceState.Engaged, immediate);
            activeHand = check;
            currentPosition = XRInteractionController.GetInteractionInformation(check).hit.point;
            startPosition = currentPosition;
            previousPosition = currentPosition;
        }

        public override void SelectStay(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            VerticalScrolling(scrollDirection, Value(check, direct: true), direct: true);
        }

        public override void SelectEnd(XRInputController.Check check, bool immediate = false, bool direct = false)
        {
            Transition(direct ? normal : engaged, direct ? InterfaceState.Disengaged : InterfaceState.Engaged, immediate);
        }
        public override void SetState(bool state)
        {
            label.SetText(state ? labelText : "");
        }
        
        private float Value(XRInputController.Check check = XRInputController.Check.Head, bool direct = false)
        {
            if (direct) return DirectDelta() * directScrollMultiplier;
            
            switch (scrollType)
            {
                case ScrollType.Delta:
                    return XRInputController.ValueDelta(XRInputController.Delta.PrimaryAxis).GetDelta(check) * scrollMultiplier;
                case ScrollType.Hold:
                    return XRInputController.AxisValue(check, truncated: true).y * scrollMultiplier;
                default:
                    return value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="input"></param>
        /// <param name="direct"></param>
        private void VerticalScrolling(ScrollDirection direction, float input, bool direct = false)
        {
            value = Mathf.Lerp(value, input, scrollSmoothing);

            XRDebug.DrawRay($"{gameObject.GetInstanceID()} Start", startPosition, -transform.forward, .05f, Color.magenta);
            XRDebug.DrawLine($"{gameObject.GetInstanceID()} Connection", currentPosition, previousPosition, Color.magenta);
            
            if (direct)
            {
                UnityScrollRect.content.localPosition += new Vector3(0, value, 0);
                return;
            }
            
            switch (direction)
            {
                case ScrollDirection.Direct:
                    UnityScrollRect.content.localPosition -= new Vector3(0, value, 0);
                    break;
                case ScrollDirection.Inverted:
                    UnityScrollRect.content.localPosition += new Vector3(0, value, 0);
                    break;
                default:
                    return;
            }
        }
    }
}
