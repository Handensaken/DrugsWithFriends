using UnityEngine;


namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
    [CreateAssetMenu(menuName = "Weapon Behaviour/Sword Behaviour")]
    public class SwordBehaviour : WeaponBehaviour
    {
        public override void DealDamage()
        {
            base.DealDamage();
            Debug.Log("Sword dealt damage!");
        }
    }
}
