using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.Locomotion
{
    public class LocomotionGhost : MonoBehaviour
    {
        [SerializeField] private Transform offset, head, left, right;
        
        private Transform referenceHead, referenceLeft, referenceRight, player;
        private bool active;
        
        private static XRInputController XRInputController => Reference.XRInputController();

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            player = Reference.Player().transform;
            referenceHead = XRInputController.Transform(XRInputController.Check.Head);
            referenceLeft = XRInputController.Transform(XRInputController.Check.Left);
            referenceRight = XRInputController.Transform(XRInputController.Check.Right);
        }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (!active) return;
            offset.transform.SplitPosition(player);
            head.LocalTransforms(referenceHead);
            left.LocalTransforms(referenceLeft);
            right.LocalTransforms(referenceRight);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void EnableGhost()
        {
            active = true;
            SetGhostVisualState(true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void DisableGhost()
        {
            active = false;
            SetGhostVisualState(false);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetGhostVisualState(bool state)
        {
            offset.gameObject.SetActive(state);
        }
    }
}
