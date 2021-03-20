using System;
using System.Linq;
using DG.Tweening;
using Project2.Scripts.Interfaces;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.Interactives
{
    public class Hazard : MonoBehaviour, InteractiveObjectInterfaces.ICanGrab, InteractiveObjectInterfaces.ITakeDamage
    {
        private Outline outline;
        [Header("Settings")]
        [SerializeField, Range(0, 100)] private float startHealth = 10f;
        [SerializeField, Range(0, 100)] private float explosionRadius = 35f;
        [SerializeField, Range(0, 100)] private float explosionForce = 75f;
        [SerializeField] private GameObject explosionVisual;
        [Header("Debug")]
        public float currentHealth;
        public bool exceededDamage;
        private Vector3 explosionLocation;

        private void Start()
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.enabled = false;

            currentHealth = startHealth;
        }
        
        public void Grab(Color color)
        {
            outline.OutlineColor = new Color(1, .3f, .5f, .2f);
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
            if (exceededDamage) return;
            
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
            // This has to be here or it will cause a stack overflow exception in case this is caught in a reciprocal explosion!
            exceededDamage = true;
            
            Vector3 position = transform.position;
            
            foreach (Collider crossfire in Physics.OverlapSphere(position, explosionRadius))
            {
                if (crossfire.gameObject == gameObject || crossfire.attachedRigidbody == null) continue;
                Debug.Log($"{crossfire.name} was caught in the explosion of {name}!");
                
                crossfire.attachedRigidbody.AddForce((crossfire.ClosestPoint(position) - position) * explosionForce, ForceMode.Impulse);
                
                if (crossfire.TryGetComponent(out InteractiveObjectInterfaces.ITakeDamage damage))
                {
                    Debug.Log($"<b>{crossfire.name} was damaged by the explosion of {name}!</b>");
                    damage.TakeDamage(explosionForce * Vector3.Distance(crossfire.ClosestPoint(position), position));
                }
                else
                {
                    Debug.Log($"{crossfire.name} could not be damaged by the explosion of {name}!");
                }
            }

            explosionLocation = transform.position;

            GameObject explosion = Instantiate(explosionVisual, null);
            explosion.transform.position = explosionLocation;
            explosion.transform.ScaleFactor(0f);
            explosion.transform.DOScale(Set.ScaleFactor(explosionRadius), .5f).OnComplete(
                () =>
                explosion.transform.DOScale(Vector3.zero, .75f).OnComplete(() => gameObject.SetActive(false)));
        }

        private void OnDrawGizmos()
        {
            if (!exceededDamage) return;
            Gizmos.color = new Color(1, .3f, .5f, .2f);
            Gizmos.DrawSphere(explosionLocation, explosionRadius);
        }
    }
}
