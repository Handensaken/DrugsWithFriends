using System.Linq;

namespace StateMachine.Solid.Scripts.Conditions
{
    public class AnyCondition : MultipleConditions
    {
        public AnyCondition(ITransitionCondition[] conditions) : base(conditions) {}

        public override bool Evaluate() => conditions.Any(condition => condition.Evaluate());
    }
}