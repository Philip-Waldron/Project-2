using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Movement
{
    public class ThrusterCooldownVisual : MonoBehaviour
    {
        private static readonly int Cooldown = Shader.PropertyToID("_Cooldown");
        private MeshRenderer Renderer => GetComponent<MeshRenderer>();

        public void SetCooldown(float cooldown)
        {
            Renderer.material.SetFloat(Cooldown, cooldown);
        }
    }
}