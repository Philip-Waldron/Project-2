using System;
using Project2.Scripts.Interfaces;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Project2.Scripts.Interactives
{
    public class Attachable : MonoBehaviour, InteractiveObjectInterfaces.ICanAttach
    {
        private Outline outline;

        private void Awake()
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.enabled = false;
        }

        public void Attach(Color color)
        {
            outline.OutlineColor = color;
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.enabled = true;
        }

        public void Detach()
        {
            outline.enabled = false;
        }
    }
}
