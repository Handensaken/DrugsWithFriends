using StateMachine.Solid.Scripts.States;

namespace StateMachineScripts.States
{
    public interface INetworkAgentBehaviour
    {
        StateType NetStateType {get; set;}
    }
}