using Scenes.Dev_Scenes.Patrik.HealthSystem;
using Scenes.Dev_Scenes.Patrik.TakeDamage;
using UnityEngine;

namespace HealthSystem.OtherHealth
{
    public class DamagePlayer : Damage
    {
        protected override void TriggerDamage(Collider collider)
        {
            if (collider.CompareTag("Enemy") && collider.TryGetComponent(out IEffectData t))
            {
                HealthManager.Instance.ChangePlayerHealthAmount(OwnerId, -1);
            }
        }
    }
}