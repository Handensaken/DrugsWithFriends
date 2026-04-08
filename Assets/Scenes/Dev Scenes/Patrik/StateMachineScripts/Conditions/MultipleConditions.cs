namespace StateMachine.Solid.Scripts.Conditions
{
    public abstract class MultipleConditions : BasicConditionCreation, ITransitionCondition
    {
        protected ITransitionCondition[] conditions;
        
        public MultipleConditions(ITransitionCondition[] conditions)
        {
            this.conditions = conditions;
        }

        public abstract bool Evaluate();
    }
}