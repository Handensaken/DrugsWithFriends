using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.Scripts.Transitions;

namespace StateMachineScripts.Structure
{
    public interface IStateTransitionFactory
    {
        public IStateTransitions GetAStateTransitions(StateType stateType);
        public void ApplyStateTransitions();
    }
}