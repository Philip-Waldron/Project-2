using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class XRPlayerCollider : MonoBehaviour
    {
        public Transform head;
        [SerializeField] private CapsuleCollider playerCollider;
        private Vector3 HeadLocalPosition => head.localPosition;

        private void Update()
        {
            playerCollider.height = HeadLocalPosition.y + 0.2f;
            playerCollider.center = new Vector3(HeadLocalPosition.x, 0.1f + HeadLocalPosition.y/2, HeadLocalPosition.z);
        }
    }
}