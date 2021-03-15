using UnityEngine;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Utilities.XR_Debug
{
    public class XRDebugRay : XRDebugLine
    {
        private Transform rayParent, rayEnd;
        
        public void DrawRay(Vector3 position, Vector3 direction, float distance, Color color)
        {
            if (!XRDebug.Enabled) return;
            
            if (!created)
            {
                rayParent = Set.Object(gameObject, name: $"[XR Debug Ray] {debugIndex}", position).transform;
                rayEnd = Set.Object(rayParent.gameObject, name: "[XR Debug Ray End]", RayPosition(distance)).transform;
            }
            SetRayTransform(position, direction, distance);
            DrawLine(from: rayParent.position, to: rayEnd.position, color);
        }

        private void SetRayTransform(Vector3 position, Vector3 direction, float distance)
        {
            rayParent.localPosition = RayPosition(distance);
            rayParent.position = position;
            rayParent.forward = direction;
        }
        private static Vector3 RayPosition(float distance)
        {
            return new Vector3(0, 0, distance);
        }
    }
}