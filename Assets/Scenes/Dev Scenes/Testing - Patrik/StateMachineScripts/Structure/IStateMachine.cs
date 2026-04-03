using StateMachine.Solid.Scripts.Transitions;

namespace StateMachineScripts.Structure
{
    public interface IStateMachine
    {
        public void SetState(IStateTransitions state);
        public IStateTransitions GetCurrentState { get; }
        public void Update();
    }
}