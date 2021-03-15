using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Common.XR_Manipulation.Snapping
{
    public class SnapVisual : XRInputAbstraction
    {
        private LineRenderer snapLine;
        private static readonly int SnapLocation = Shader.PropertyToID("_SnapLocation");
        private static readonly int SnapDistance = Shader.PropertyToID("_SnapDistance");
        /// <summary>
        /// 
        /// </summary>
        public void CreateSnapVisual(string parent)
        {
            snapLine = Set.Object(parent: null, name: $"[{parent} Snap Visual]", Vector3.zero).Line(XRManipulationController.snapVisualMaterial, .005f, startEnabled: false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="snapDistance"></param>
        public void SetSnapVisualState(bool state, Vector3 from, Vector3 to, float snapDistance)
        {
            snapLine.DrawLine(from, to);
            snapLine.enabled = state;
            snapLine.material.SetVector(SnapLocation, to);
            snapLine.material.SetFloat(SnapDistance, snapDistance);
        }
    }
}