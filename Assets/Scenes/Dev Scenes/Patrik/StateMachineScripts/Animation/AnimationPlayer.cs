using StateMachine.Solid.Scripts.Animation;
using StateMachine.Solid.Scripts.States;
using UnityEngine;

namespace StateMachineScripts.Animation
{
    public class AnimationPlayer : IAnimationPlayer
    {
        private readonly Animator animator;
        private readonly AnimationData animationData;

        public AnimationPlayer(Animator animator, AnimationData animationData)
        {
            this.animator = animator;
            this.animationData = animationData;
        }
        
        public void TryPlay(StateType type)
        {
            if (animator is null)
            {
                Debug.Log("Animator is null: Failed to play animation");
                return;
            }
            
            if (animationData is null)
            {
                Debug.Log("AnimatorData is null: Failed to play animation");
                return;
            }
            
            foreach (StateAnimation stateAnimation in animationData.StateAnimations)
            {
                if (stateAnimation.StateType == type)
                {
                    animator.Play(stateAnimation.AnimationClip.name);
                    Debug.Log("Found - Animation");
                    return;
                }
            }
            
            Debug.Log("Did not find animation to play");
        }
    }
}