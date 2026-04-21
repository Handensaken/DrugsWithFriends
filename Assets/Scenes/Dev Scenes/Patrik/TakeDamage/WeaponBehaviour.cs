using UnityEngine;


namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
    public class WeaponBehaviour : ScriptableObject
    {
        public int damage;
        public virtual void DealDamage() // All weapons should be able to deal damage
        {
            
        }
    }
}
