using Project2.Scripts.Utilities.XR_Debug;
using Project2.Scripts.XR_Player.Common.Feedback;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Interaction;
using Project2.Scripts.XR_Player.Common.XR_Interface;
using Project2.Scripts.XR_Player.Common.XR_Manipulation;
using UnityEngine;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Interaction;
using XR_Prototyping.Scripts.Common.XR_Interface;
using XR_Prototyping.Scripts.Common.XR_Manipulation;
using XR_Prototyping.Scripts.Utilities.XR_Debug;

namespace XR_Prototyping.Scripts.Common.XR_Input
{
    public class XRInputAbstraction : MonoBehaviour
    {
        protected static XRInputController XRInputController => Reference.XRInputController();
        protected static XRInteractionController XRInteractionController => Reference.XRInteractionController();
        protected static XRManipulationController XRManipulationController => Reference.XRManipulationController();
        protected static XRFeedbackController XRFeedbackController => Reference.XRFeedbackController();
        protected static XRVisualEffectsController XRVisualEffectsController => Reference.XRControllerInterfaceController();
        protected static XRDebug XRDebug => Reference.XRGizmos();
    }
}
