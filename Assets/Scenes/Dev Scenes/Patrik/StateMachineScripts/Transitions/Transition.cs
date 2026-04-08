using System;
using StateMachine.Solid.Transitions;

namespace StateMachine.Solid.Scripts.Transitions
{
    public class Transition : ITransition
    {
        public IStateTransitions ToState { get; }
        public Func<bool> Condition { get; }

        public Transition(IStateTransitions toState, Func<bool> condition)
        {
            ToState = toState;
            Condition = condition;
        }
    }
}