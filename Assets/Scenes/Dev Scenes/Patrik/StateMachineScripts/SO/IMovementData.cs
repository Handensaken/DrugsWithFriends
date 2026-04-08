namespace StateMachine.Solid.Scripts.SO
{
    public interface IMovementData
    {
        public float Speed{ get; }
        public float AngularSpeed{ get; }
        public float Acceleration{ get; }
    }
}