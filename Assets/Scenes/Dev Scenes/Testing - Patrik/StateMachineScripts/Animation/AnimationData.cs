using UnityEngine;

namespace StateMachine.Solid.Scripts.Animation
{
    [CreateAssetMenu(menuName = "Solid-Machine/Agent/Animation/AnimationData")]
    public class AnimationData : ScriptableObject
    {
        [SerializeField] private StateAnimation[] stateAnimations;
        public StateAnimation[] StateAnimations => stateAnimations;
    }
}