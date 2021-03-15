using System.Collections;
using System.Collections.Generic;
using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;
using XR_Prototyping.Scripts.Common;
using XR_Prototyping.Scripts.Common.XR_Input;

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
}
