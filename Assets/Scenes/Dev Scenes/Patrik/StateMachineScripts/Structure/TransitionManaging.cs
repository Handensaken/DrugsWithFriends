using System;
using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;

namespace StateMachineScripts.Structure
{
    public class TransitionManaging : ITransitionManaging
    {
        public void ApplyTransitions(IStateTransitions fromState, ITransition[] transitions)
        {
            Apply(fromState, transitions);
        }

        private void Apply(IStateTransitions fromState, ITransition[] transitions)
        {
            foreach (var transition in transitions)
            {
                if (transition.ToState == null || transition.Condition == null)
                    throw new Exception($"Incomplete transition in use {GetType().Name}");
                
                fromState.AddTransition(transition.ToState, transition.Condition);
            }
        }
    }
}