using System;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using XR_Prototyping.Scripts.Utilities.Extensions;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    [RequireComponent(typeof(Collider))]
    public class XRAttachableObject : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float mass = 1f, drag = 10f;
        
        public Collider Collider => GetComponent<Collider>();
        public Rigidbody Rigidbody { get; private set; }
        public HingeJoint HingeJoint => GetComponent<HingeJoint>();
        
        private Outline outline;

        private void Awake()
        {
            Rigidbody = gameObject.AddConfiguredRigidbody(mass, drag);
            
            outline = gameObject.AddComponent<Outline>();
            outline.precomputeOutline = true;
            outline.OutlineWidth = 10f;
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.OutlineColor = Color.white;
            outline.enabled = false;
        }

        public void Target()
        {
            
        }

        public void Attach(Rigidbody player)
        {
            HingeJoint.connectedBody = player;
        }
        
        public void Detach()
        {
            HingeJoint.connectedBody = null;
        }
    }
}
