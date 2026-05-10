using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircle : MonoBehaviour
    {
        [SerializeField,Min(0.1f)] private float circleRange;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, circleRange);
        }
    }
}