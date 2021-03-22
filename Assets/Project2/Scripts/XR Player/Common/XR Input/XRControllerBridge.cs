using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace XR_Prototyping.Scripts.Common
{
    public class XRControllerBridge : MonoBehaviour
    {
        [SerializeField] private XRInputController.Check xrControllerCheck;

        public XRInputController.Check XRControllerCheck()
        {
            return xrControllerCheck;
        }
    }
}
