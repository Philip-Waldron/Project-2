using Project2.Scripts.XR_Player.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface;
using Object = UnityEngine.Object;

namespace Project2.Scripts.XR_Player.Common.XR_Interface
{
    public class XRVisualEffectsController : XRInputAbstraction
    {
        private XRControllerInterface dominantControllerInterface, nonDominantControllerInterface;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="target"></param>
        /// <param name="controllerInterface"></param>
        public void SetInterface(XRInputController.Hand hand, XRControllerInterface.Interface target, XRControllerInterface.ControllerInterface controllerInterface)
        {
            XRControllerInterface controller = XRControllerInterface(hand);
            if (Valid(controller))
            {
                controller.SetInterface(target, controllerInterface);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private XRControllerInterface XRControllerInterface(XRInputController.Hand hand)
        {
            return hand == XRInputController.Hand.Dominant ? dominantControllerInterface : nonDominantControllerInterface;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllerInterface"></param>
        /// <returns></returns>
        private static bool Valid(Object controllerInterface)
        {
            return controllerInterface != null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="controllerInterface"></param>
        public void SetControllerInterface(XRInputController.Check check, XRControllerInterface controllerInterface)
        {
            if (check == XRInputController.DominantHand())
            {
                dominantControllerInterface = controllerInterface;
            }
            else if(check == XRInputController.NonDominantHand())
            {
                nonDominantControllerInterface = controllerInterface;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetVignetteStrength(float strength)
        {
            // postProcessVolume.profile.TryGetSettings(out Vignette vignette);
            // vignette.intensity.Override(strength);
        }
    }
}