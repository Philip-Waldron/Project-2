using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.XR_Player.Common.XR_Interface.XR_Menu
{
    public class AnchoredMenuController : XRMenu
    {
        private enum AnchoredMenuBehaviour
        {
            Static,
            Rotatable
        }
        [Header("Anchored Menu Settings")] 
        [SerializeField] private AnchoredMenuBehaviour anchoredMenuBehaviour = AnchoredMenuBehaviour.Static;
        [SerializeField] private XRInputController.Cardinal clockwiseDirection = XRInputController.Cardinal.Left, antiClockwiseDirection = XRInputController.Cardinal.Right;
        [SerializeField, Range(0f, 1f)] private float rotateSpeed = .1f, menuElementsRadius = .2f;
        [SerializeField] private Set.Axis menuAxis = Set.Axis.Up;
        [Header("Anchored Menu References")] 
        [SerializeField] private Transform rotationOrigin;
        [SerializeField] private List<XRMenuComponent> menuComponents = new List<XRMenuComponent>();
        
        private float RotateAngle => 360f / menuComponents.Count;
        private int CurrentIndex => menuComponents.FindIndex(match: index => index == currentMenuSection);
        private int IndexCap => menuComponents.Count - 1;

        private XRMenuComponent currentMenuSection;
        private bool clockwise, cooldown;
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int NextMenuSectionIndex()
        {
            int uncheckedIndex = clockwise ? CurrentIndex - 1 : CurrentIndex + 1;
            uncheckedIndex = uncheckedIndex > IndexCap ? 0 : uncheckedIndex;
            uncheckedIndex = uncheckedIndex < 0 ? IndexCap : uncheckedIndex;
            return uncheckedIndex;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuAwake() { }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuStart()
        {
            currentMenuSection = menuComponents.FirstOrDefault();
            SetMenuComponentStates();
            OrientateAnchoredElements();
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void MenuUpdate()
        {
            if (anchoredMenuBehaviour == AnchoredMenuBehaviour.Static) return;
            OrientateAnchoredElements();
            if (cooldown || !TriggerStart()) return;
            RotateMenu();
        }
        /// <summary>
        /// 
        /// </summary>
        private void RotateMenu()
        {
            cooldown = true;
            rotationOrigin.DOLocalRotate(endValue: LocalRotation(), rotateSpeed).OnComplete(FindCurrentMenuSection);
        }
        /// <summary>
        /// 
        /// </summary>
        private void FindCurrentMenuSection()
        {
            currentMenuSection = menuComponents[NextMenuSectionIndex()];
            SetMenuComponentStates();
            cooldown = false;
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetMenuComponentStates()
        {
            foreach (XRMenuComponent element in menuComponents)
            {
                element.SetMenuComponentState(element == currentMenuSection);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 LocalRotation()
        {
            switch (menuAxis)
            {
                case Set.Axis.Right:
                    return new Vector3(Angle(), 0f, 0f);
                case Set.Axis.Up:
                    return new Vector3(0f, Angle(), 0f);
                case Set.Axis.Forward:
                    return new Vector3(0f, 0f, Angle());
                default:
                    return Vector3.zero;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float Angle()
        {
            switch (menuAxis)
            {
                case Set.Axis.Right:
                    return rotationOrigin.localEulerAngles.x + (clockwise ? RotateAngle : -RotateAngle);
                case Set.Axis.Up:
                    return rotationOrigin.localEulerAngles.y + (clockwise ? RotateAngle : -RotateAngle);
                case Set.Axis.Forward:
                    return rotationOrigin.localEulerAngles.z + (clockwise ? RotateAngle : -RotateAngle);
                default:
                    return 0f;
            }
        }

        protected override void MenuSummonStart() { }

        protected override void MenuSummonStay() { }

        protected override void MenuSummonEnd() { }

        protected override bool TriggerStart()
        {
            if (XRInputController.AxisDirection(XRInputController.CheckHand(menuType.attachedHand), clockwiseDirection))
            {
                clockwise = true;
                return true;
            }
            if (XRInputController.AxisDirection(XRInputController.CheckHand(menuType.attachedHand), antiClockwiseDirection))
            {
                clockwise = false;
                return true;
            }
            return false;
        }

        protected override bool TriggerEnd() { return false; }

        public override void ResetMenu() { }
        private void OnValidate()
        {
            OrientateAnchoredElements();
        }
        private List<Transform> MenuElements => menuComponents.Select(element => element.transform).ToList();
        /// <summary>
        /// 
        /// </summary>
        private void OrientateAnchoredElements()
        {
            if (rotationOrigin == null) return;
            rotationOrigin.CircularPositioning(MenuElements, menuAxis, menuElementsRadius);
        }
    }
}
