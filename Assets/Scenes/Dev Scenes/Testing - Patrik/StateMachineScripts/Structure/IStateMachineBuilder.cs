using StateMachine.Solid.Scripts.States;
using StateMachineScripts.States;

namespace StateMachineScripts.Structure
{
    public interface IStateMachineBuilder
    {
        public IStateFactory GetAStateFactory(StateType stateType);
    }
}