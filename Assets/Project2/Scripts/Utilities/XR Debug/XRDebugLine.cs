using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;

namespace XR_Prototyping.Scripts.Utilities.XR_Debug
{
    public class XRDebugLine : BaseXRDebug
    {
        private LineRenderer line;
        
        public void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            if (!XRDebug.Enabled) return;
            
            if (!created)
            {
                line = gameObject.Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth, color);
                created = true;
            }
            line.material.color = color;
            line.DrawLine(from, to);
            line.enabled = XRDebug.Enabled;
        }
    }
}