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
    public class AttackTransitionFactory : ITransitionFactory
    {
        private readonly ISightData chaseSightData;
        private readonly IAttackData attackAttackData;
        private readonly Transform sightTransform;
        private readonly IStateTransitions patrol;
        private readonly IStateTransitions chase;
        
        public AttackTransitionFactory(ISightData chaseSightData,IAttackData attackAttackData, Transform sightTransform, IStateTransitions patrol, IStateTransitions chase)
        {
            this.chaseSightData = chaseSightData;
            this.attackAttackData = attackAttackData;
            this.sightTransform = sightTransform;
            this.patrol = patrol;
            this.chase = chase;
        }
        
        public ITransition[] CreateTransition()
        {
            ICertifiedTargetProvider chaseCertifiedTargetProvider = new CertifiedTargetProvider(new InSightTargetFilter(sightTransform, chaseSightData).Filter);
            ITransitionCondition outOfRange = new SubsetNotInRangeCondition(sightTransform,chaseCertifiedTargetProvider, attackAttackData.MaxAttackRange);
            ITransitionCondition chaseInSight = new InSightCondition(sightTransform, chaseCertifiedTargetProvider, chaseSightData);
            
            ITransitionCondition attackToChase = new AllCondition(new []{outOfRange,chaseInSight});
            ITransition toChase = new Transition(chase, attackToChase.Evaluate);
            
            ITransition toPatrol = new Transition(patrol, outOfRange.Evaluate);
            
            return new[] {toChase,toPatrol};
        }
    }
}