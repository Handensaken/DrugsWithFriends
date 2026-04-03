using StateMachine.Solid.Scripts.SO;
using UnityEngine;

namespace Solid.Scripts.SO
{
    [CreateAssetMenu(menuName = "StateMachine/Agent/AgentData")]
    public class AgentData : ScriptableObject
    {
        [SerializeField] private StateMovementData patrolMovementData;
        public StateMovementData PatrolMovementData => patrolMovementData;
        
        [SerializeField] private StateSightData patrolSightData;
        public StateSightData PatrolSightData => patrolSightData;

        [SerializeField] private StateMovementData chaseMovementData;
        public StateMovementData ChaseMovementData => chaseMovementData;
        
        [SerializeField] private StateSightData chaseSightData;
        public StateSightData ChaseSightData => chaseSightData;
        
        [SerializeField] private AttackData attackData;
        public AttackData AttackData => attackData;
    }
}
