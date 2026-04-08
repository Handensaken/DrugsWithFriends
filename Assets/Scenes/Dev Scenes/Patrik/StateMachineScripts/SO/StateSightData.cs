using StateMachine.Solid.ScriptableObjects;
using UnityEngine;

namespace StateMachine.Solid.Scripts.SO
{
    [CreateAssetMenu(menuName = "Solid-Machine/Agent/SightData")]
    public class StateSightData : ScriptableObject, ISightData
    {
        [SerializeField] private float sightRange;
        public float SightRange => sightRange;
        
        [SerializeField] private float sightAngle;
        public float SightAngle => sightAngle;
    }
}