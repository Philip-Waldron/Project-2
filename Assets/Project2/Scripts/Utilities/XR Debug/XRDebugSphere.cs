using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;
using Orientation = XR_Prototyping.Scripts.Utilities.LineRendererExtension.Orientation;

namespace XR_Prototyping.Scripts.Utilities.XR_Debug
{
    public class XRDebugSphere : BaseXRDebug
    {
        private LineRenderer rightCircle, upCircle, forwardCircle;

        public void DrawSphere(Vector3 center, float radius, Color color, int quality = 64)
        {
            if (!XRDebug.Enabled) return;
            
            if (!created)
            {
                rightCircle = SetupCircle("[Right Circle]", color);
                upCircle = SetupCircle("[Up Circle]", color);
                forwardCircle = SetupCircle("[Forward Circle]", color);
                created = true;
            }

            SetSpherePosition(transform,  center);
            DrawCircle(rightCircle, radius, Orientation.Right, quality);
            DrawCircle(upCircle, radius, Orientation.Up, quality);
            DrawCircle(forwardCircle, radius, Orientation.Forward, quality);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="center"></param>
        private static void SetSpherePosition(Transform sphere, Vector3 center)
        {
            sphere.position = center;
            sphere.eulerAngles = Vector3.zero;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="circleName"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private LineRenderer SetupCircle(string circleName, Color color)
        {
            LineRenderer circle = Set.Object(gameObject, circleName, Vector3.zero).Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth);
            circle.material.color = color;
            return circle;
        }
        /// <summary>
        /// 
        /// </summary>
        private static void DrawCircle(LineRenderer circle, float radius, Orientation orientation, int quality)
        {
            // circle.widthMultiplier = Vector3.Distance() * XRDebug.XRDebugLineWidth;
            circle.DrawCircle(radius, orientation, quality);
            circle.enabled = XRDebug.Enabled;
        }
    }
}