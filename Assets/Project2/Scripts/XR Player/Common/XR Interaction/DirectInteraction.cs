using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;

namespace XR_Prototyping.Scripts.Common.XR_Interaction
{
    public class DirectInteraction : XRInputAbstraction
    {
        public Check check;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handed"></param>
        public void SetupDirectInteraction(Check handed)
        {
            check = handed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out XRInterfaceAbstraction interfaceAbstraction))
            {
                interfaceAbstraction.SelectStart(check, immediate: false, direct: true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.TryGetComponent(out XRInterfaceAbstraction interfaceAbstraction))
            {
                interfaceAbstraction.SelectStay(check, immediate: false, direct: true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out XRInterfaceAbstraction interfaceAbstraction))
            {
                interfaceAbstraction.SelectEnd(check, immediate: false, direct: true);
            }
        }
    }
}