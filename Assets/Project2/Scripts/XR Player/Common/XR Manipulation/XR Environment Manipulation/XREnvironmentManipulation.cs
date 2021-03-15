using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Input.Input_Data;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Manipulation;
using XR_Prototyping.Scripts.Utilities.Generic;
using Check = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.Check;
using XRControllerButton = Project2.Scripts.XR_Player.Common.XR_Input.XRInputController.XRControllerButton;

namespace Project2.Scripts.XR_Player.Common.XR_Manipulation.XR_Environment_Manipulation
{
    public class XREnvironmentManipulation : XRInputAbstraction
    {
        [Header("XR Environment Manipulation Settings")] 
        [SerializeField] private XREnvironmentalManipulationType environmentalManipulationType;
        //--------------------------------------------------------------------------------------------------------------
        private Check environmentManipulationCheck;
        private Check BimanualManipulationCheck => XRInputController.OtherHand(environmentManipulationCheck);
        //--------------------------------------------------------------------------------------------------------------
        private Transform nonDominantManipulationProxy, dominantManipulationProxy, bimanualManipulationProxy, scalingBimanualProxy, scalingProxy;
        //--------------------------------------------------------------------------------------------------------------
        public bool Manipulating { get; private set; }
        public bool Bimanual { get; private set; }
        public bool BimanualListening { get; private set; }
        private Vector3 initialScale = Vector3.one;
        //--------------------------------------------------------------------------------------------------------------
        public float ScaleFactor => ProxyScale.magnitude / initialScale.magnitude;
        //--------------------------------------------------------------------------------------------------------------
        private bool LockedAxis => environmentalManipulationType.objectOrientationBehaviour == XREnvironmentalManipulationType.ObjectOrientationBehaviour.LockedVertical;
        private bool BimanualOnly => environmentalManipulationType.manipulationTrigger == XREnvironmentalManipulationType.ManipulationTrigger.BimanualOnly;
        private bool Scalable => environmentalManipulationType.objectScalingBehaviour != XREnvironmentalManipulationType.ObjectScalingBehaviour.NoScaling;
        private bool ShouldClamp => environmentalManipulationType.objectScalingBehaviour == XREnvironmentalManipulationType.ObjectScalingBehaviour.ClampedScaling;
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Should we clamp the scaling of the environment?
        /// </summary>
        /// <returns></returns>
        private bool ClampScaling()
        {
            return ShouldClamp && (OverSized || UnderSized);
        }
        private Vector3 cachedScale = Vector3.one;
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 ClampedScaling()
        {
            cachedScale = ClampScaling() ? cachedScale : scalingProxy.localScale;
            return cachedScale;
        }
        private bool OverSized => ProxyScale.magnitude > Set.ScaleFactor(environmentalManipulationType.maximumScale).magnitude;
        private bool UnderSized => ProxyScale.magnitude < Set.ScaleFactor(environmentalManipulationType.minimumScale).magnitude;
        private Vector3 ProxyScale => scalingProxy.transform.lossyScale;
        //--------------------------------------------------------------------------------------------------------------
        private Vector3 BimanualScale => XRManipulationController.GetManipulationInformation(environmentManipulationCheck, bimanual: true).proxy.transform.localScale;

        private void Awake()
        {
            nonDominantManipulationProxy = Set.Object(gameObject, "[XR Environmental Manipulation Proxy] Non Dominant", Vector3.zero).transform;
            dominantManipulationProxy = Set.Object(gameObject, "[XR Environmental Manipulation Proxy] Dominant", Vector3.zero).transform;
            bimanualManipulationProxy = Set.Object(gameObject, "[XR Environmental Manipulation Proxy] Bimanual", Vector3.zero).transform;
            scalingBimanualProxy = Set.Object(gameObject, "[XR Environmental Manipulation Proxy] Bimanual Scaling", Vector3.zero).transform;
            scalingProxy = Set.Object(null, "[XR Environmental Manipulation] Environment Proxy", Vector3.zero).transform;
            // InitialScale = EnvironmentScale;
        }
        private void Update()
        {
            SetProxyInformation();
            ManipulationLogic();
            return;
            if (ShouldClamp && Manipulating)
            {
                // environment.transform.localScale = ClampedScaling();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetProxyInformation()
        {
            // scalingProxy.Transforms(environment.transform);
            ManipulationProxyBehaviour(dominantManipulationProxy, check: XRInputController.DominantHand());
            ManipulationProxyBehaviour(nonDominantManipulationProxy, check: XRInputController.NonDominantHand());
            ManipulationProxyBehaviour(bimanualManipulationProxy, Check.Head, bimanual: true);
            ManipulationProxyBehaviour(scalingBimanualProxy, Check.Head, bimanual: true, scaling: true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool Valid()
        {
            return !XRManipulationController.Manipulating();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckBimanual()
        {
            return !BimanualOnly && environmentalManipulationType.bimanualManipulationBehaviour == XREnvironmentalManipulationType.BimanualManipulationBehaviour.AllowBimanual;
        }
        /// <summary>
        /// 
        /// </summary>
        private void ManipulationLogic()
        {
            if (!Valid()) return;
            // When you are already manipulating the environment
            if (Manipulating)
            {
                // Call the stay method
                ManipulateEnvironmentStay();
                // Encapsulated logic for determining bimanual state
                CheckBimanualState();
                // Check to release the environment
                if (EnvironmentalManipulationTriggerEnd())
                {
                    ManipulateEnvironmentEnd();
                }
            }
            // If you aren't manipulating, then you can check to start manipulating
            else if (EnvironmentalManipulationTriggerStart())
            {
                ManipulateEnvironmentStart();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void CheckBimanualState()
        {
            if (!CheckBimanual()) return;
            if (EnvironmentalBimanualManipulationTriggerEnd())
            {
                BimanualEnvironmentEnd();
            }
            else if (EnvironmentalBimanualManipulationTriggerStart())
            {
                BimanualEnvironmentStart();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ManipulateEnvironmentStart()
        {
            Manipulating = true;
            XRManipulationController.Enabled = false;
            BimanualListening = false;
            scalingProxy.transform.SetParent(ManipulationProxy(environmentManipulationCheck));
            StartManipulating(ManipulationProxy(environmentManipulationCheck));
            if (BimanualOnly)
            {
                BimanualEnvironmentStart();
            }
        }
        private bool CanSwap => environmentalManipulationType.manipulationTrigger == XREnvironmentalManipulationType.ManipulationTrigger.Bimanual;
        /// <summary>
        /// 
        /// </summary>
        private void ManipulateEnvironmentStay()
        {
            // environment.ManipulateStay();
            
            // Logic for swapping parents goes here!
            if (ShouldClamp && Bimanual && Scalable)
            {
                // environment.SetManipulationParent(ClampScaling() ? bimanualManipulationProxy : scalingBimanualProxy);
            }
            // If you are able to swap hands
            if (CanSwap && XRInputController.InputEvent(XRControllerButton.Grip).State(environmentManipulationCheck, InputEvents.InputEvent.Transition.Up))
            {
                environmentManipulationCheck = BimanualManipulationCheck;
                scalingProxy.transform.SetParent(ManipulationProxy(environmentManipulationCheck));
                SetParent(ManipulationProxy(environmentManipulationCheck));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ManipulateEnvironmentEnd()
        {
            BimanualEnvironmentEnd();
            Manipulating = false;
            BimanualListening = false;
            XRManipulationController.Enabled = true;
            scalingProxy.SetParent(null);
            StopManipulating();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        private static void StartManipulating(Transform parent)
        {
            foreach (XRManipulatableElement element in FindObjectsOfType<XRManipulatableElement>())
            {
                element.ManipulateStart(parent);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        private static void SetParent(Transform parent)
        {
            foreach (XRManipulatableElement element in FindObjectsOfType<XRManipulatableElement>())
            {
                element.SetManipulationParent(parent);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private static void StopManipulating()
        {
            foreach (XRManipulatableElement element in FindObjectsOfType<XRManipulatableElement>())
            {
                element.ManipulateEnd();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void BimanualEnvironmentStart()
        {
            Bimanual = true;
            BimanualListening = false;
            scalingProxy.transform.SetParent(scalingBimanualProxy);
            SetParent(scalingBimanualProxy);
        }
        /// <summary>
        /// 
        /// </summary>
        private void BimanualEnvironmentEnd()
        {
            Bimanual = false;
            BimanualListening = false;
            scalingProxy.transform.SetParent(ManipulationProxy(environmentManipulationCheck));
            SetParent(ManipulationProxy(environmentManipulationCheck));
        }
        private static bool BimanualGrabbing => XRInputController.Grab(XRInputController.NonDominantHand()) && XRInputController.Grab(XRInputController.NonDominantHand());
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EnvironmentalManipulationTriggerStart()
        {
            bool grabbed = XRInputController.InputEvent(XRControllerButton.Grip).State(InputEvents.InputEvent.Transition.Down, out Check grabHand);
            
            switch (environmentalManipulationType.manipulationTrigger)
            {
                // You have grabbed this frame, and it matches the defined grab trigger hand
                case XREnvironmentalManipulationType.ManipulationTrigger.Unimanual when grabHand == XRInputController.CheckHand(environmentalManipulationType.environmentalManipulationHand):
                    environmentManipulationCheck = grabHand;
                    return grabbed;
                // You have grabbed this frame, no logic for checking which hand it is is needed
                case XREnvironmentalManipulationType.ManipulationTrigger.Bimanual:
                    environmentManipulationCheck = grabHand;
                    return grabbed;
                case XREnvironmentalManipulationType.ManipulationTrigger.BimanualOnly:
                    // If you are listening and you press down with the other
                    if (BimanualListening)
                    {
                        // todo this could be made to equal Transition.Stay
                        // If you release the hand you originally grabbed with
                        if (XRInputController.InputEvent(XRControllerButton.Grip).State(environmentManipulationCheck, InputEvents.InputEvent.Transition.Up))
                        {
                            BimanualListening = false;
                            return false;
                        }
                        // You have grabbed with the other hand
                        if (XRInputController.InputEvent(XRControllerButton.Grip).State(BimanualManipulationCheck, InputEvents.InputEvent.Transition.Down))
                        {
                            return true;
                        }
                    }
                    // If you have grabbed in this frame, but it's the first hand you've grabbed with
                    else if (grabbed)
                    {
                        environmentManipulationCheck = grabHand;
                        BimanualListening = true;
                        return false;
                    }
                    // Otherwise just return false
                    return false;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EnvironmentalManipulationTriggerEnd()
        {
            switch (environmentalManipulationType.manipulationTrigger)
            {
                case XREnvironmentalManipulationType.ManipulationTrigger.Unimanual:
                    return XRInputController.InputEvent(XRControllerButton.Grip).State(environmentManipulationCheck, InputEvents.InputEvent.Transition.Up); 
                case XREnvironmentalManipulationType.ManipulationTrigger.Bimanual:
                    return XRInputController.InputEvent(XRControllerButton.Grip).State(environmentManipulationCheck, InputEvents.InputEvent.Transition.Up); 
                case XREnvironmentalManipulationType.ManipulationTrigger.BimanualOnly:
                    return !BimanualGrabbing;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EnvironmentalBimanualManipulationTriggerStart()
        {
            return XRInputController.InputEvent(XRControllerButton.Grip).State(BimanualManipulationCheck, InputEvents.InputEvent.Transition.Down);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EnvironmentalBimanualManipulationTriggerEnd()
        {
            return XRInputController.InputEvent(XRControllerButton.Grip).State(BimanualManipulationCheck, InputEvents.InputEvent.Transition.Up);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="check"></param>
        /// <param name="bimanual"></param>
        /// <param name="scaling"></param>
        private void ManipulationProxyBehaviour(Transform proxy, Check check, bool bimanual = false, bool scaling = false)
        {
            Vector3 position = proxy.position;
            XRDebug.DrawRay(index: $"{proxy.GetInstanceID()} Forward", position, proxy.forward, .015f, Color.cyan);
            XRDebug.DrawRay(index: $"{proxy.GetInstanceID()} Right", position, proxy.right, .01f, Color.magenta);
            XRDebug.DrawRay(index: $"{proxy.GetInstanceID()} Up", position, proxy.up, .01f, Color.yellow);
            proxy.Transforms(XRManipulationController.GetManipulationInformation(check, bimanual).proxy.transform);
            // See if we rescale the bimanual proxy's scale
            proxy.localScale = Scalable && bimanual && scaling ? BimanualScale : Vector3.one;
            if (!LockedAxis) return;
            proxy.LockRotationAboutAxis(Set.Axis.Up);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private Transform ManipulationProxy(Check check)
        {
            return check == XRInputController.DominantHand() ? dominantManipulationProxy : nonDominantManipulationProxy;
        }
    }
}