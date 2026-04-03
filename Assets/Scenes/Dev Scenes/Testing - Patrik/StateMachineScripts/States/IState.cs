namespace StateMachineScripts.States
{
    public interface IState
    {
        public void Enter();
        public void Update();
        public void UpdateVisualization();
        public void Exit();
    }
}