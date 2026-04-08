using System;
using StateMachine.Solid.Transitions;
using StateMachineScripts.States;

namespace StateMachine.Solid.Scripts.Transitions
{
    public interface IStateTransitions
    {
        public IState GetState { get; }
        public void AddTransition(IStateTransitions changeToState, Func<bool> condition);
        public ITransition[] GetTransitions { get; }

    }
}