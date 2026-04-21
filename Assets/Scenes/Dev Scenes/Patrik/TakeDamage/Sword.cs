using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
    public class Sword : NetworkBehaviour, IEffectData
    {
        public int damage;
        public float knockbackForce;

        public void ApplyEffect(Rigidbody rb, Collider collider)
        {
            // Direction from the weapon collider toward the dummy
            Vector3 knockbackDirection = (rb.position - collider.transform.position).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }
}