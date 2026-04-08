using StateMachine.Solid.Scripts.States;

namespace StateMachineScripts.Animation
{
    public interface IAnimationPlayer
    {
        public void TryPlay(StateType type);
    }
}