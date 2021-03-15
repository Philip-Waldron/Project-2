using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interaction;
using XR_Prototyping.Scripts.Common.XR_Interface;
using XR_Prototyping.Scripts.Utilities.XR_Debug;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;

namespace XR_Prototyping.Scripts.Utilities
{
    public static class IndirectInteraction
    {
        private static XRDebug XRDebug => Reference.XRGizmos();
        private static XRInputController XRInputController => Reference.XRInputController();
        private static XRInteractionController XRInteractionController => Reference.XRInteractionController();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="direction"></param>
        /// <param name="raycastHit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(XRInputController.Check check, Vector3 direction, out RaycastHit raycastHit, float distance = 1f)
        {
            bool valid = false;
            if (Physics.Raycast(XRInputController.Position(check), direction, out RaycastHit hit, distance))
            {
                raycastHit = hit;
                valid = true;
            }
            raycastHit = valid ? hit : new RaycastHit();
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="direction"></param>
        /// <param name="layerMask"></param>
        /// <param name="raycastHit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(Check check, Vector3 direction, out RaycastHit raycastHit, float distance, LayerMask layerMask)
        {
            bool valid = false;
            
            // layerMask = 1 << layerMask;
            
            if (Physics.Raycast(XRInputController.Position(check), direction, out RaycastHit hit, distance, layerMask))
            {
                raycastHit = hit;
                valid = true;
            }
            
            Debug.DrawRay(XRInputController.Position(check), direction * distance, valid ? Color.green : Color.red);
            
            raycastHit = valid ? hit : new RaycastHit();
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="direction"></param>
        /// <param name="target"></param>
        /// <param name="raycastHit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(Check check, Vector3 direction, Collider target, out RaycastHit raycastHit, float distance = 1f)
        {
            bool valid = false;
            if (Physics.Raycast(XRInputController.Position(check), direction, out RaycastHit hit, distance) && hit.collider == target)
            {
                raycastHit = hit;
                valid = true;
            }
            raycastHit = valid ? hit : new RaycastHit();
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="target"></param>
        /// <param name="raycastHit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(Vector3 position, Vector3 direction, Collider target, out RaycastHit raycastHit, float distance = 1f)
        {
            bool valid = false;
            if (Physics.Raycast(position, direction, out RaycastHit hit, distance) && hit.collider == target)
            {
                raycastHit = hit;
                valid = true;
            }
            raycastHit = valid ? hit : new RaycastHit();
            return valid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="target"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(Check check, Collider target, float distance = 1f)
        {
            if (Physics.Raycast(XRInputController.Position(check), XRInputController.Forward(check), out RaycastHit hit, distance))
            {
                return hit.collider == target;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="check"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(Collider target, out Check check, float distance = 1f)
        {
            if (Physics.Raycast(XRInputController.Position(Check.Left), XRInputController.Forward(Check.Left), out RaycastHit leftHit, distance))
            {
                check = Check.Left;
                return leftHit.collider == target;
            }
            if (Physics.Raycast(XRInputController.Position(Check.Right), XRInputController.Forward(Check.Right), out RaycastHit rightHit, distance))
            {
                check = Check.Right;
                return rightHit.collider == target;
            }
            check = Check.Head;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="direction"></param>
        /// <param name="check"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Hover(Collider target, Vector3 direction, out Check check, float distance = 1f)
        {
            if (Physics.Raycast(XRInputController.Position(Check.Left), direction, out RaycastHit leftHit, distance))
            {
                check = Check.Left;
                return leftHit.collider == target;
            }
            if (Physics.Raycast(XRInputController.Position(Check.Right), direction, out RaycastHit rightHit, distance))
            {
                check = Check.Right;
                return rightHit.collider == target;
            }
            check = Check.Head;
            return false;
        }
        /// <summary>
        /// This is a method which raycasts from each of the user's controllers and returns any valid raycast hits
        /// </summary>
        /// <returns></returns>
        public static bool Hover(out bool left, out bool right, out RaycastHit leftRaycastHit, out RaycastHit rightRaycastHit, LayerMask layerMask, float distance = 1f)
        {
            left = false;
            right = false;
            leftRaycastHit = new RaycastHit();
            rightRaycastHit = new RaycastHit();
            
            if (Physics.Raycast(XRInputController.Position(Check.Left), XRInputController.Forward(Check.Left, false), out RaycastHit leftHit, distance, layerMask))
            {
                left = true;
                leftRaycastHit = leftHit;
            }
            if (Physics.Raycast(XRInputController.Position(Check.Right), XRInputController.Forward(Check.Right, false), out RaycastHit rightHit, distance, layerMask))
            {
                right = true;
                rightRaycastHit = rightHit;
            }
            
            return left || right;
        }

        [Serializable] public struct InteractionInformation
        {
            public float interactionDistance;
            public RaycastHit hit;
            public XRInterfaceAbstraction currentXRInterface;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="raycastHit"></param>
            /// <param name="distance"></param>
            /// <param name="abstraction"></param>
            public void SetInformation(RaycastHit raycastHit, float distance, XRInterfaceAbstraction abstraction)
            {
                hit = raycastHit;
                interactionDistance = distance;
                currentXRInterface = abstraction;
            }
        }
        /// <summary>
        /// This is a method which raycasts from each of the user's controllers and returns any valid raycast hits
        /// </summary>
        /// <returns></returns>
        public static bool InteractionHover(out bool left, out bool right, out InteractionInformation leftInformation, out InteractionInformation rightInformation, LayerMask layerMask, float maximumDistance = 1f, float minimumDistance = .25f)
        {
            left = false;
            right = false;
            leftInformation = new InteractionInformation();
            rightInformation = new InteractionInformation();

            // right = IndirectInformation(Check.Right, out InteractionInformation rightCache, maximumDistance, minimumDistance, layerMask);
            // left = IndirectInformation(Check.Left, out InteractionInformation leftCache, maximumDistance, minimumDistance, layerMask);
            // 
            // rightInformation = rightCache;
            // leftInformation = leftCache;

            if (XRInteractionController.InteractionInitialised(Check.Left))
            {
                XRDebug.DrawSphere($"Indirect Interaction Min Left", XRInteractionController.GetXRInteractionOrigin(Check.Left).Position(), minimumDistance, Color.cyan);
                XRDebug.DrawRay($"Indirect Interaction Left", XRInteractionController.GetXRInteractionOrigin(Check.Left).Position(), XRInteractionController.GetXRInteractionOrigin(Check.Left).Forward(), maximumDistance, Color.cyan);

                if (Physics.Raycast(
                    origin: XRInteractionController.GetXRInteractionOrigin(Check.Left).Position(), 
                    direction: XRInteractionController.GetXRInteractionOrigin(Check.Left).Forward(), 
                    out RaycastHit leftHit, maximumDistance, layerMask))
                {
                    if (leftHit.transform.TryGetComponent(out XRInterfaceAbstraction abstraction) && Vector3.Distance(XRInteractionController.GetXRInteractionOrigin(Check.Left).Position(), leftHit.point) >= minimumDistance)
                    {
                        XRDebug.DrawRay(index: $"Indirect Interaction, Raycast Hit Reverse Normal, Left", leftHit.point, leftHit.normal, .05f, Color.magenta);
                        left = true;
                        leftInformation.SetInformation(leftHit, Vector3.Distance(XRInteractionController.GetXRInteractionOrigin(Check.Left).Position(), leftHit.point), abstraction);
                    }
                }
            }

            if (XRInteractionController.InteractionInitialised(Check.Right))
            {
                XRDebug.DrawSphere($"Indirect Interaction Min Right", XRInteractionController.GetXRInteractionOrigin(Check.Right).Position(), minimumDistance, Color.cyan);
                XRDebug.DrawRay($"Indirect Interaction Right", XRInteractionController.GetXRInteractionOrigin(Check.Right).Position(), XRInteractionController.GetXRInteractionOrigin(Check.Right).Forward(), maximumDistance, Color.cyan);

                if (Physics.Raycast(
                    origin: XRInteractionController.GetXRInteractionOrigin(Check.Right).Position(), 
                    direction: XRInteractionController.GetXRInteractionOrigin(Check.Right).Forward(), 
                    out RaycastHit rightHit, maximumDistance, layerMask))
                {
                    if (rightHit.transform.TryGetComponent(out XRInterfaceAbstraction abstraction) && Vector3.Distance(XRInteractionController.GetXRInteractionOrigin(Check.Right).Position(), rightHit.point) >= minimumDistance)
                    {
                        XRDebug.DrawRay(index: $"Indirect Interaction, Raycast Hit Reverse Normal, Right", rightHit.point, rightHit.normal, .05f, Color.magenta);
                        right = true;
                        rightInformation.SetInformation(rightHit, Vector3.Distance(XRInteractionController.GetXRInteractionOrigin(Check.Right).Position(), rightHit.point), abstraction);
                    }
                }
            }

            return left || right;
        }

        private static bool IndirectInformation(Check check, out InteractionInformation information, float maximumDistance, float minimumDistance, LayerMask layerMask)
        {
            information = new InteractionInformation();
            if (!XRInteractionController.InteractionInitialised(check)) return false;
            
            XRDebug.DrawSphere($"Indirect Interaction Max {check}", XRInteractionController.GetXRInteractionOrigin(check).Position(), maximumDistance, Color.magenta);
            XRDebug.DrawSphere($"Indirect Interaction Min {check}", XRInteractionController.GetXRInteractionOrigin(check).Position(), minimumDistance, Color.magenta);
            XRDebug.DrawRay($"Indirect Interaction {check}", XRInteractionController.GetXRInteractionOrigin(check).Position(), XRInteractionController.GetXRInteractionOrigin(check).Forward(), maximumDistance, Color.magenta);

            if (Physics.Raycast(
                    origin: XRInteractionController.GetXRInteractionOrigin(check).Position(), 
                    direction: XRInteractionController.GetXRInteractionOrigin(check).Forward(), 
                    out RaycastHit newHit, minimumDistance, layerMask))
            {
                if (newHit.transform.TryGetComponent(out XRInterfaceAbstraction abstraction) && Vector3.Distance(XRInteractionController.GetXRInteractionOrigin(check).Position(), newHit.point) >= minimumDistance)
                {
                    XRDebug.DrawRay(index: $"Indirect Interaction Raycast Normal {check}", newHit.point, newHit.normal, .05f, Color.magenta);
                    information.SetInformation(newHit, Vector3.Distance(XRInteractionController.GetXRInteractionOrigin(check).Position(), newHit.point), abstraction);
                    return true;
                }
            }
            return false;
        }
    }
}
