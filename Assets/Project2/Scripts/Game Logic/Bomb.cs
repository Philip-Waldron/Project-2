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
            Collider.enabled = false;
        }

        public void Eject(Vector3 velocity, GameController controller)
        {
            gameController = controller;
            transform.SetParent(null);
            Collider.enabled = true;
            Rigidbody.velocity = velocity;
        }

        public void Couple(Vector3 velocity)
        {
            Collider.enabled = false;
            Rigidbody.velocity = velocity;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.transform.TryGetComponent(out Objective objective))
            {
                gameController.Collision(collision.relativeVelocity.magnitude);
            }
        }
    }
}
