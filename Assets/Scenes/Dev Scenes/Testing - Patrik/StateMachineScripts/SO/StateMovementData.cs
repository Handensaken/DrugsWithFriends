using UnityEngine;

namespace StateMachine.Solid.Scripts.SO
{
    [CreateAssetMenu(menuName = "Solid-Machine/Agent/MovementData")]
    public class StateMovementData : ScriptableObject, IMovementData 
    {
        [SerializeField] private float movementSpeed;
        public float Speed => movementSpeed;
        
        [SerializeField] private float angularSpeed;
        public float AngularSpeed => angularSpeed;

        [SerializeField] private float acceleration;
        public float Acceleration => acceleration;
    }
}