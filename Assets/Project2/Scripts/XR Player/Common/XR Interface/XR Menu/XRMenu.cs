using System;
using UnityEngine;
using UnityEngine.Events;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu.Menu_Type;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu
{
    public abstract class XRMenu : XRInputAbstraction
    {
        [Header("References")]
        [SerializeField] protected Transform menuParent;
        [SerializeField] protected MenuType menuType;
        [Header("Menu Events")] 
        [SerializeField] protected UnityEvent menuStart;
        [SerializeField] protected UnityEvent menuEnd;
        
        protected XRInterfaceAbstraction[] interfaceElements;
        protected bool latch = false;
        public bool Enabled { get; set; }

        private void Awake()
        {
            MenuAwake();
        }
        private void Start()
        {
            interfaceElements = menuParent.GetComponentsInChildren<XRInterfaceAbstraction>();
            SetMenuState(menuType.triggerType == MenuType.TriggerType.Persistent);
            MenuStart();
        }
        private void Update()
        {
            if (menuType.movementBehaviour == MenuType.MovementBehaviour.Attached)
            {
                SetMenuPosition();
            }
            SetMenuState(Enabled);
            MenuUpdate();
        }
        /// <summary>
        /// 
        /// </summary>
        protected abstract void MenuAwake();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void MenuStart();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void MenuUpdate();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void MenuSummonStart();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void MenuSummonStay();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void MenuSummonEnd();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool TriggerStart();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool TriggerEnd();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        protected void SetMenuState(bool state)
        {
            Enabled = state;
            
            if (Enabled)
            {
                menuStart.Invoke();
            }
            else
            {
                menuEnd.Invoke();
            }
            
            SetMenuStateWithoutNotify(Enabled);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void SetMenuStateWithoutNotify(bool state)
        {
            if (interfaceElements == null) return;
            foreach (XRInterfaceAbstraction element in interfaceElements)
            {
                element.SetState(state);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public abstract void ResetMenu();
        /// <summary>
        /// 
        /// </summary>
        protected void SetMenuPosition()
        {
            Transform menuTransform = transform;
            Vector3 rotation = XRInputController.Rotation(XRInputController.CheckHand(menuType.attachedHand));
            menuTransform.Position(XRInputController.Transform(XRInputController.CheckHand(menuType.attachedHand)));
            switch (menuType.menuOrientation)
            {
                case MenuType.MenuOrientation.FullyAligned:
                    menuTransform.eulerAngles = rotation;
                    break;
                case MenuType.MenuOrientation.VerticallyAligned:
                    menuTransform.eulerAngles = new Vector3(rotation.x, rotation.y, 0f);
                    break;
                case MenuType.MenuOrientation.Vertical:
                    menuTransform.eulerAngles = new Vector3(0f, rotation.y, 0f);
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public void SetMenuType(MenuType type)
        {
            menuType = type;
        }
    }
}
