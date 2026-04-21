using System.Collections;
using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
    public class Sword : NetworkBehaviour, IEffectData
    {
        public int damage;
        public float knockbackForce;
        [Range(0, 2f), Tooltip("How long the enemies should have the knockback applied")] public float knockbackTime;

        public void ApplyEffect(Rigidbody rb, Collider collider)
        {
            Vector3 knockbackDirection = (rb.position - collider.transform.position).normalized;
            
            knockbackDirection.y = 0f;
            knockbackDirection.Normalize();
    
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            StartCoroutine(ResetRigidBody(rb));
        }

        private IEnumerator ResetRigidBody(Rigidbody rb)
        {
            yield return new WaitForSeconds(knockbackTime);
            rb.linearVelocity = Vector3.zero;
        }
    }
}