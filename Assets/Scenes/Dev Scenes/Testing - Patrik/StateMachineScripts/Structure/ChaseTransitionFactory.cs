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
    public class ChaseTransitionFactory : ITransitionFactory
    {
        private readonly ISightData chaseSightData;
        private readonly IAttackData attackAttackData;
        private readonly Transform sightTransform;
        private readonly IStateTransitions patrol;
        private readonly IStateTransitions attack;
        
        public ChaseTransitionFactory(ISightData chaseSightData, IAttackData attackAttackData, Transform sightTransform, IStateTransitions patrol, IStateTransitions attack)
        {
            this.chaseSightData = chaseSightData;
            this.attackAttackData = attackAttackData;
            this.sightTransform = sightTransform;
            this.patrol = patrol;
            this.attack = attack;
        }
        
        public ITransition[] CreateTransition()
        {
            ICertifiedTargetProvider chaseCertifiedTargetProvider = new CertifiedTargetProvider(new InSightTargetFilter(sightTransform, chaseSightData).Filter);
            
            ITransitionCondition chaseNotInSight = new NotInSightCondition(sightTransform, chaseCertifiedTargetProvider, chaseSightData);
            ITransition toPatrol = new Transition(patrol, chaseNotInSight.Evaluate);
            
            ITransitionCondition chaseInRangeForAttack = new SubsetInRangeCondition(sightTransform,chaseCertifiedTargetProvider,attackAttackData.MinAttackRange);
            ITransition toAttack = new Transition(attack, chaseInRangeForAttack.Evaluate);
            
            return new[] {toPatrol, toAttack};
        }
    }
}