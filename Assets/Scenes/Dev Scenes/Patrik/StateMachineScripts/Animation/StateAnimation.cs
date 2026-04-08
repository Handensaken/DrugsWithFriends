using System;
using StateMachine.Solid.Scripts.States;
using UnityEngine;

namespace StateMachine.Solid.Scripts.Animation
{
    [Serializable]
    public struct StateAnimation
    {
        [SerializeField] private StateType stateType;
        public StateType StateType => stateType;
        
        [SerializeField] private AnimationClip animationClip;
        public AnimationClip AnimationClip => animationClip;
    }
}