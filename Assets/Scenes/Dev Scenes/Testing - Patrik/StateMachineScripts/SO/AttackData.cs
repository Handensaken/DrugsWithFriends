using StateMachine.Solid.ScriptableObjects;
using UnityEngine;

namespace StateMachine.Solid.Scripts.SO
{
    [CreateAssetMenu(menuName = "Solid-Machine/Agent/AttackData")]
    public class AttackData : ScriptableObject, IAttackData
    {
        [SerializeField,Min(0)] private float minAttackRange;
        public float MinAttackRange => minAttackRange;
        
        [SerializeField,Min(0)] private float maxAttackRange;
        public float MaxAttackRange => maxAttackRange;

        private void OnValidate()
        {
            if (maxAttackRange < minAttackRange)
            {
                maxAttackRange = minAttackRange;
            }
        }
    }
}