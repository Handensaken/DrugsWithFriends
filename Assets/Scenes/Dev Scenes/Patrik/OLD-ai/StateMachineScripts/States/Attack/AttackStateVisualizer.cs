using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.SO;
using UnityEngine;

namespace StateMachine.Solid.Scripts.States.Attack
{
    public class AttackStateVisualizer : BaseStateVisualizer, IStateVisualizer
    {
        private readonly IAttackData attackData;
        
        public AttackStateVisualizer(Color stateColor, Transform sightTransform, IAttackData attackData) : base(stateColor, sightTransform)
        {
            this.attackData = attackData;
        }

        public override void Visualize()
        {
            base.Visualize();
            VisualizeRange();
        }

        private void VisualizeRange()
        {
            Gizmos.DrawWireSphere(SightTransform.position, attackData.MinAttackRange);
            Gizmos.DrawWireSphere(SightTransform.position, attackData.MaxAttackRange);
        }
    }
}