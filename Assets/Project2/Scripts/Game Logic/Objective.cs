using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace Project2.Scripts.Game_Logic
{
    public class Objective : MonoBehaviour
    {
        [SerializeField] private Transform recordLocation;
        [SerializeField, Range(.25f, 3f)] private float animationDuration = 1f;
        
        private Rigidbody Rigidbody => GetComponent<Rigidbody>();

        public UnityEvent metObjective;
        
        private void OnCollisionEnter(Collision other)
        {
            Debug.Log($"Objective collided with {other.transform.name}!");
            
            if (other.transform.TryGetComponent(out Bomb bomb))
            {
                bomb.AttachToObjective(Rigidbody.velocity);
                Complete(bomb);
            }
        }

        private void Complete(Bomb bomb)
        {
            bomb.transform.SetParent(recordLocation);
            bomb.transform.DOLocalMove(Vector3.zero, animationDuration).OnComplete(() => bomb.Rigidbody.isKinematic = true);
            bomb.transform.DOLocalRotate(Vector3.zero, animationDuration).OnComplete(()=> metObjective.Invoke());
            bomb.transform.DOScale(Vector3.one, animationDuration).OnComplete(() => bomb.transform.ResetLocalTransform());
        }
    }
}
