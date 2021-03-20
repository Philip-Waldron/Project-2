using Project2.Scripts.Interfaces;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Project2.Scripts.Interactives
{
    public class Grabbable : MonoBehaviour, InteractiveObjectInterfaces.ICanGrab
    {
        private Outline outline;

        private void Awake()
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.enabled = false;
        }
        
        public void Grab(Color color)
        {
            outline.OutlineColor = color;
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.enabled = true;
        }

        public void Release()
        {
            outline.enabled = false;
        }
    }
}
