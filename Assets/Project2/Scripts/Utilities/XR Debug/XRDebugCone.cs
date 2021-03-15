using Project2.Scripts.Utilities.XR_Debug;
using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;
using Orientation = XR_Prototyping.Scripts.Utilities.LineRendererExtension.Orientation;

namespace XR_Prototyping.Scripts.Utilities.XR_Debug
{
    public class XRDebugCone : BaseXRDebug
    {
        private LineRenderer coneBaseCircle, generatorLeftLine, generatorRightLine, generatorTopLine, generatorBottomLine;
        private Transform coneBase, generatorLeft, generatorRight, generatorTop, generatorBottom;
        public Calculations.Cone cone;

        private Vector3 LeftPosition => new Vector3(-cone.Radius, 0f, 0f);
        private Vector3 RightPosition => new Vector3(cone.Radius, 0f, 0f);
        private Vector3 TopPosition => new Vector3(0f, cone.Radius, 0f);
        private Vector3 BottomPosition => new Vector3(0f, -cone.Radius, 0f);
        
        public void DrawCone(Transform origin, float angle, float height, Color color, int quality = 64)
        {
            if (!XRDebug.Enabled) return;
            
            cone = new Calculations.Cone(origin, angle, height);
            
            if (!created)
            {
                // Setup the cone base
                coneBase = Set.Object(gameObject, "[Cone Base]", Vector3.zero).transform;
                coneBaseCircle = SetupCircle(coneBase.gameObject, "[Cone Base Circle]", color);
                // Setup the generators
                generatorLeft = Set.Object(coneBase.gameObject, "[Cone Generator] Left", LeftPosition).transform;
                generatorRight = Set.Object(coneBase.gameObject, "[Cone Generator] Right", RightPosition).transform;
                generatorTop = Set.Object(coneBase.gameObject, "[Cone Generator] Top", TopPosition).transform;
                generatorBottom = Set.Object(coneBase.gameObject, "[Cone Generator] Bottom", BottomPosition).transform;
                generatorLeftLine = generatorLeft.gameObject.Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth, color);
                generatorRightLine = generatorRight.gameObject.Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth, color);
                generatorTopLine = generatorTop.gameObject.Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth, color);
                generatorBottomLine = generatorBottom.gameObject.Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth, color);
                // Finish 
                created = true;
            }

            SetConePosition();
            DrawCircle(coneBaseCircle, cone.Radius, Orientation.Forward, quality);
            generatorLeftLine.DrawLine(transform, generatorLeft, XRDebug.Enabled);
            generatorRightLine.DrawLine(transform, generatorRight, XRDebug.Enabled);
            generatorTopLine.DrawLine(transform, generatorTop, XRDebug.Enabled);
            generatorBottomLine.DrawLine(transform, generatorBottom, XRDebug.Enabled);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetConePosition()
        {
            Transform coneTransform = transform;
            coneTransform.position = cone.coneTipPosition;
            coneTransform.forward = cone.coneAxis;
            coneBase.localPosition = ConeBasePosition(cone.coneHeight);
            generatorLeft.localPosition = LeftPosition;
            generatorRight.localPosition = RightPosition;
            generatorTop.localPosition = TopPosition;
            generatorBottom.localPosition = BottomPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circleParent"></param>
        /// <param name="circleName"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static LineRenderer SetupCircle(GameObject circleParent, string circleName, Color color)
        {
            LineRenderer circle = Set.Object(circleParent, circleName, Vector3.zero).Line(XRDebug.debugMaterial, XRDebug.XRDebugLineWidth);
            circle.material.color = color;
            return circle;
        }
        /// <summary>
        /// 
        /// </summary>
        private static void DrawCircle(LineRenderer circle, float radius, Orientation orientation, int quality)
        {
            circle.DrawCircle(radius, orientation, quality);
            circle.enabled = XRDebug.Enabled;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        private static Vector3 ConeBasePosition(float distance)
        {
            return new Vector3(0, 0, distance);
        }
    }
}