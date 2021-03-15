using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace XR_Prototyping.Scripts.Common.XR_Interface.XR_Menu
{
    public class XRMenuComponent : MonoBehaviour
    {
        [Header("XR Menu Component")]
        [SerializeField] private UnityEvent setActive;
        [SerializeField] private UnityEvent setInactive;
        private List<XRInterfaceAbstraction> menuInterfaceElements = new List<XRInterfaceAbstraction>();

        private void Awake()
        {
            menuInterfaceElements = GetComponentsInChildren<XRInterfaceAbstraction>().ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetMenuComponentState(bool state)
        {
            foreach (XRInterfaceAbstraction element in menuInterfaceElements)
            {
                element.gameObject.SetActive(state);
                //element.SetState(state);
            }
            if (state)
            {
                setActive.Invoke();
            }
            else
            {
                setInactive.Invoke();
            }
        }
    }
}