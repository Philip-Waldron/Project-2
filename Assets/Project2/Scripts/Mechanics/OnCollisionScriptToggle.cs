using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionScriptToggle : MonoBehaviour
{
    public bool alwaysToggleToTrue = true;
    public List<MonoBehaviour> scriptsToEnable;

    void OnCollisionEnter(Collision collision)
    {
        foreach (var script in scriptsToEnable) 
        {
            if (alwaysToggleToTrue)
            {
                script.enabled = true;
            }
            else
            {
                script.enabled = !script.enabled;
            }
        }
    }
}
