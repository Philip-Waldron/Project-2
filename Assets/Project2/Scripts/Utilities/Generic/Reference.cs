using System.Collections;
using System.Collections.Generic;
using Project2.Scripts.XR_Player.Common.XR_Input;
using Project2.Scripts.XR_Player.Common.XR_Interface;
using UnityEngine;
using XR_Prototyping.Scripts.Common;
using XR_Prototyping.Scripts.Common.Feedback;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Common.XR_Interaction;
using XR_Prototyping.Scripts.Common.XR_Interface;
using XR_Prototyping.Scripts.Common.XR_Manipulation;
using XR_Prototyping.Scripts.Utilities.XR_Debug;

public static class Reference
{
    public const string PlayerTag = "Player", DominantHandShaderReference = "_DH", NonDominantHandShaderReference = "_NDH";
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static GameObject Player()
    {
        return GameObject.FindWithTag(PlayerTag);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Camera Camera()
    {
        return UnityEngine.Camera.main;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XRInputController XRInputController()
    {
        return Player().GetComponent<XRInputController>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XRInteractionController XRInteractionController()
    {
        return Player().GetComponent<XRInteractionController>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XRFeedbackController XRFeedbackController()
    {
        return Player().GetComponent<XRFeedbackController>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XRManipulationController XRManipulationController()
    {
        return Player().GetComponent<XRManipulationController>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XRVisualEffectsController XRControllerInterfaceController()
    {
        return Player().GetComponent<XRVisualEffectsController>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static XRDebug XRGizmos()
    {
        return Player().GetComponent<XRDebug>();
    }
}
