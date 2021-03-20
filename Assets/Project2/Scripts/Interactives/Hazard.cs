using System;
using System.Linq;
using Project2.Scripts.Interfaces;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Project2.Scripts.Interactives
{
    public class Hazard : MonoBehaviour, InteractiveObjectInterfaces.ICanGrab, InteractiveObjectInterfaces.ITakeDamage
    {
        private Outline outline;
        [Header("Settings")]
        [SerializeField, Range(0, 100)] private float startHealth = 10f;
        [SerializeField, Range(0, 100)] private float explosionRadius = 35f;
        [SerializeField, Range(0, 100)] private float explosionForce = 75f;
        [Header("Debug")]
        public float currentHealth;
        public bool exceededDamage;

        private void Awake()
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.enabled = false;

            currentHealth = startHealth;
        }
        
        public void Grab(Color color)
        {
            outline.OutlineColor = Color.red;
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            outline.enabled = true;
        }

        public void Release()
        {
            outline.enabled = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            TakeDamage(collision.relativeVelocity.magnitude);
        }

        public void TakeDamage(float damage)
        {
            
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, startHealth);

            if (currentHealth <= 0f)
            {
                ExceedDamage();
            }
        }

        public void ExceedDamage()
        {
            if (exceededDamage) return;
            
            Vector3 position = transform.position;
            foreach (Collider crossfire in Physics.OverlapSphere(position, explosionRadius).Where(target => target.attachedRigidbody != null))
            {
                crossfire.attachedRigidbody.AddForce((crossfire.ClosestPoint(position) - position) * explosionForce, ForceMode.Impulse);
                if (crossfire.TryGetComponent(out InteractiveObjectInterfaces.ITakeDamage damage))
                {
                    damage.TakeDamage(explosionForce * Vector3.Distance(crossfire.ClosestPoint(position), position));
                }
            }

            exceededDamage = true;
        }

        private void OnDrawGizmos()
        {
            if (!exceededDamage) return;
            Gizmos.color = new Color(1, .3f, .5f, .2f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}
