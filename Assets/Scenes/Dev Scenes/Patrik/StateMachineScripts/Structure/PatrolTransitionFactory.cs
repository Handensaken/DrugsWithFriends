using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.Conditions;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.Targets;
using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;
using StateMachineScripts.Conditions;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachine.Solid.Scripts.StateMachine
{
    public class PatrolTransitionFactory : ITransitionFactory
    {
        private readonly ISightData patrolSightData;
        private readonly IAttackData attackAttackData;
        private readonly Transform sightTransform;
        private readonly IStateTransitions chase;
        private readonly IStateTransitions attack;
        
        public PatrolTransitionFactory(ISightData patrolSightData,IAttackData attackAttackData, Transform sightTransform, IStateTransitions chase, IStateTransitions attack)
        {
            this.patrolSightData = patrolSightData;
            this.attackAttackData = attackAttackData;
            this.sightTransform = sightTransform;
            this.chase = chase;
            this.attack = attack;
        }
        
        public ITransition[] CreateTransition()
        {
            ICertifiedTargetProvider patrolTargetProvider = new CertifiedTargetProvider(new InSightTargetFilter(sightTransform, patrolSightData).Filter);
            
            ITransitionCondition patrolInSight = new InSightCondition(sightTransform, patrolTargetProvider, patrolSightData);
            ITransition toChase = new Transition(chase, patrolInSight.Evaluate);
            
            ITransitionCondition patrolInRangeAttack = new SubsetInRangeCondition(sightTransform,patrolTargetProvider,attackAttackData.MinAttackRange);
            ITransitionCondition patrolInSightAttackRange = new AllCondition(new []{patrolInSight,patrolInRangeAttack});
            ITransition toAttack = new Transition(attack, patrolInSightAttackRange.Evaluate);
            
            
            return new[] {toAttack, toChase};
        }
    }
}