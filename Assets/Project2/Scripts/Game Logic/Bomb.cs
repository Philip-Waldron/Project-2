using System;
using UnityEngine;

namespace Project2.Scripts.Game_Logic
{
    public class Bomb : MonoBehaviour
    {
        private Collider Collider => GetComponent<Collider>();
        public Rigidbody Rigidbody => GetComponent<Rigidbody>();

        private GameController gameController;
        
        private void Awake()
        {
            Collider.enabled = true;
            Rigidbody.AddForce(transform.up, ForceMode.Impulse);
        }

        public void Couple(GameController controller)
        {
            gameController = controller;
            Collider.enabled = false;
        }

        public void Decouple(Vector3 velocity)
        {
            transform.SetParent(null);
            Collider.enabled = true;
            Rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        public void AttachToObjective(Vector3 velocity)
        {
            Collider.enabled = false;
            Rigidbody.velocity = velocity;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Make the bomb take damage now
            if (!collision.transform.TryGetComponent(out Objective objective))
            {
                gameController.Collision(collision.relativeVelocity.magnitude);
            }
        }
    }
}