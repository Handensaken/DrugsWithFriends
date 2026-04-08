using StateMachine.Solid.Scripts.Transitions;

namespace StateMachineScripts.States
{
    public interface IStateFactory
    {
        public IStateTransitions CreateState();
    }
}

