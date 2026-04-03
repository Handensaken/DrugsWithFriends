using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;

namespace StateMachineScripts.Structure
{
    public class StateMachine: IStateMachine
    {
        private IStateTransitions currentState;

        public IStateTransitions GetCurrentState => currentState;
        
        public void Update()
        {
            if (CheckTransition(out ITransition transition))
            {
                ChangeState(transition.ToState);
            }
            
            currentState.GetState.Update();
        }

        public void SetState(IStateTransitions stateAndTransitions)
        {
            currentState = stateAndTransitions;
            currentState?.GetState.Enter();
        }
        
        private bool CheckTransition(out ITransition validTransition)
        {
            foreach (ITransition transition in currentState.GetTransitions)
            {
                if (transition.Condition.Invoke())
                {
                    validTransition = transition;
                    return true;
                }
            }
            
            validTransition = null;
            return false;
        }

        private void ChangeState(IStateTransitions changeToState)
        {
            if (changeToState == currentState) return;
            
            currentState.GetState.Exit();
            currentState = changeToState;
            currentState.GetState.Enter();
        }

        
    }
}