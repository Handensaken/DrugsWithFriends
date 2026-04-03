using System;
using System.Collections.Generic;
using StateMachine.Solid.Scripts.Transitions;
using StateMachineScripts.States;

namespace StateMachine.Solid.Transitions
{
    public class StateTransitions : IStateTransitions
    {
        private readonly IState state;
        private readonly List<ITransition> transitions;

        public StateTransitions(IState state)
        {
            this.state = state;
            transitions = new List<ITransition>();
        }

        public void AddTransition(IStateTransitions changeToState, Func<bool> condition)
        {
            transitions.Add(new Transition(changeToState, condition));
        }

        public IState GetState => state;
        public ITransition[] GetTransitions => transitions.ToArray();
    }
}