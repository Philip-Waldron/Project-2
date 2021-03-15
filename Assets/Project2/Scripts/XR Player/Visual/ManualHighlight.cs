using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Manipulation;
using UnityEngine;
using UnityEngine.UI;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Manipulation;

namespace XR_Prototyping.Scripts.Visual
{
    public class ManualHighlight : XRInputAbstraction
    {
        [SerializeField] private XRManipulationController.ManipulationHands manipulationHands = XRManipulationController.ManipulationHands.DominantOnly;
        [SerializeField] private bool mapBothHands;
        private Renderer Renderer => GetComponent<Renderer>();
        private Image Image => GetComponent<Image>();

        private enum Type { Renderer, Image, Null }
        private Type type;

        private void Start()
        {
            if (Renderer != null)
            {
                type = Type.Renderer;
                return;
            }
            if (Image != null)
            {
                type = Type.Image;
                return;
            }
            type = Type.Null;
        }

        private void Update()
        {
            switch (manipulationHands)
            {
                case XRManipulationController.ManipulationHands.DominantOnly:
                    SetValues(Reference.DominantHandShaderReference, XRInputController.DominantHand());
                    if (mapBothHands)
                    {
                        SetValues(Reference.NonDominantHandShaderReference, XRInputController.DominantHand());
                    }
                    return;
                case XRManipulationController.ManipulationHands.NonDominantOnly:
                    SetValues(Reference.DominantHandShaderReference, XRInputController.NonDominantHand());
                    if (mapBothHands)
                    {
                        SetValues(Reference.NonDominantHandShaderReference, XRInputController.NonDominantHand());
                    }
                    return;
                case XRManipulationController.ManipulationHands.Both:
                    SetValues(Reference.DominantHandShaderReference, XRInputController.DominantHand());
                    SetValues(Reference.NonDominantHandShaderReference, XRInputController.NonDominantHand());
                    return;
                case XRManipulationController.ManipulationHands.Neither:
                    return;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="check"></param>
        private void SetValues(string reference, XRInputController.Check check)
        {
            switch (type)
            {
                case Type.Renderer:
                    Renderer.material.SetVector(reference, XRInputController.Position(check));
                    return;
                case Type.Image:
                    Image.material.SetVector(reference, XRInputController.Position(check));
                    return;
                case Type.Null:
                    return;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceMaterial"></param>
        /// <returns></returns>
        private Material GetModifiedMaterial(Material sourceMaterial)
        {
            string[] shaderKeywords = sourceMaterial.shaderKeywords;
            Material baseMaterial = sourceMaterial;
            baseMaterial.shaderKeywords = shaderKeywords;
            return baseMaterial;
        }
    }
}
