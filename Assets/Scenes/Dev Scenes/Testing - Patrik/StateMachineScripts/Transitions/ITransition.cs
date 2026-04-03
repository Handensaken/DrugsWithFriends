using System;
using StateMachine.Solid.Scripts.Transitions;

namespace StateMachine.Solid.Transitions
{
    public interface ITransition
    {
        IStateTransitions ToState { get; }
        Func<bool> Condition { get; }
    }
}