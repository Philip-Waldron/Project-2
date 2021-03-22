using UnityEngine;
using UnityEngine.Events;

namespace Project2.Scripts.XR_Player.Common.XR_Input
{
    public class AnalogGestureEvents : MonoBehaviour
    {
        [Header("Gesture Information")]
        [SerializeField] private XRInputController.Check check;
        [SerializeField] private UnityEvent forward, backward, left, right, center;

        private void Update()
        {
            if (XRInputController.Instance.AxisDirection(check, XRInputController.Cardinal.Forward))
            {
                forward.Invoke();
            }
            if (XRInputController.Instance.AxisDirection(check, XRInputController.Cardinal.Back))
            {
                backward.Invoke();
            }
            if (XRInputController.Instance.AxisDirection(check, XRInputController.Cardinal.Left))
            {
                left.Invoke();
            }
            if (XRInputController.Instance.AxisDirection(check, XRInputController.Cardinal.Right))
            {
                right.Invoke();
            }
            if (XRInputController.Instance.AxisDirection(check, XRInputController.Cardinal.Center))
            {
                center.Invoke();
            }
        }
    }
}
