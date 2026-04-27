using System.Collections.Generic;
using Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [CreateAssetMenu(menuName = "AI/Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Behaviours and corresponding stats")]
        [SerializeField] public PatrolPackage patrolPackage;
        [SerializeField] public ChasePackage chasePackage;
        [SerializeField] public AttackPackage attackPackage;
        
        [Space, Header("Refs")]
        [SerializeField] public HealthData healthData;

        public SightPackage[] GetALlSightData()
        {
            List<SightPackage> result = new List<SightPackage>();
            
            result.Add(patrolPackage.sightPackage);

            return result.ToArray();
        }
    }
}