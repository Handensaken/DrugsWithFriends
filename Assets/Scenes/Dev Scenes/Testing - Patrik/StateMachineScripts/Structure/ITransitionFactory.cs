using StateMachine.Solid.Transitions;

namespace StateMachine.Solid.Scripts.StateMachine
{
    public interface ITransitionFactory
    {
        public ITransition[] CreateTransition();
        
    }
}