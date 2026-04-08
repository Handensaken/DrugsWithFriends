using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;

namespace StateMachineScripts.Structure
{
    public interface ITransitionManaging
    {
        public void ApplyTransitions(IStateTransitions fromState, ITransition[] transitions);
    }
}