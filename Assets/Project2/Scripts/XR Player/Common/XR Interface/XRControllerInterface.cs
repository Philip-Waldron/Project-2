using System;
using System.Collections.Generic;
using System.Linq;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace XR_Prototyping.Scripts.Common.XR_Interface
{
    public class XRControllerInterface : XRInputAbstraction
    {
        private XRControllerBridge XRControllerBridge => GetComponentInParent<XRControllerBridge>();
        private XRInputController.Check Check => XRControllerBridge.XRControllerCheck();
        [Serializable] public struct InterfaceIndex
        {
            public Interface interfaceIndex;
            public Image image;
        }
        [Serializable] public struct ControllerInterface
        {
            public Color color;
            public Sprite sprite;
        }
        public List<InterfaceIndex> interfaces = new List<InterfaceIndex>();
        public enum Interface
        {
            AnalogInterface,
            PrimaryInterface,
            SecondaryInterface,
            GripInterface,
            MenuInterface
        }
        
        private void Start()
        {
            XRVisualEffectsController.SetControllerInterface(Check, controllerInterface: this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="controllerInterface"></param>
        public void SetInterface(Interface target, ControllerInterface controllerInterface)
        {
            foreach (InterfaceIndex element in interfaces.Where(element => element.interfaceIndex == target))
            {
                element.image.sprite = controllerInterface.sprite;
                element.image.color = controllerInterface.color;
            }
        }
    }
}