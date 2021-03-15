using TMPro;
using UnityEngine;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities.Extensions;
using XR_Prototyping.Scripts.Utilities.Generic;
using Vector3 = UnityEngine.Vector3;

namespace XR_Prototyping.Scripts.Utilities.XR_Debug
{
    public class XRDebugLog : BaseXRDebug
    {
        private TextMeshPro debugText;

        public void Log(string text, Vector3 position, Transform lookAt, float size = .1f)
        {
            if (!XRDebug.Enabled) return; 
            
            if (!created)
            {
                debugText = Set.Object(gameObject, "Log", Vector3.zero).TextMeshPro(new Vector2(.05f, .05f), XRDebug.debugFont, HorizontalAlignmentOptions.Center, VerticalAlignmentOptions.Top, size, true);
                created = true;
            }
            SetTextPosition(transform, lookAt, position);
            debugText.SetText(text);
            debugText.fontSize = size;
            debugText.gameObject.SetActive(XRDebug.Enabled); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lookAt"></param>
        /// <param name="position"></param>
        private static void SetTextPosition(Transform text, Transform lookAt, Vector3 position)
        {
            text.position = position;
            text.LookAwayFrom(lookAt, Vector3.up);
        }
    }
}