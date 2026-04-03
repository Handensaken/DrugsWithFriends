using System.Linq;

namespace StateMachine.Solid.Scripts.Conditions
{
    public class AllCondition : MultipleConditions
    {
        public AllCondition(ITransitionCondition[] conditions) : base(conditions) {}

        public override bool Evaluate() => conditions.All(condition => condition.Evaluate());
    }
}