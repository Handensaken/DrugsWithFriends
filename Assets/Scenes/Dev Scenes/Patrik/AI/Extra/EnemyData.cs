using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [CreateAssetMenu(menuName = "AI/Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] public HealthSO healthData;
    
        [Space,Header("Behaviours and corresponding stats")]
        [SerializeField] public Patrol patrol;
        [SerializeField] public Chase chase;
    }
}