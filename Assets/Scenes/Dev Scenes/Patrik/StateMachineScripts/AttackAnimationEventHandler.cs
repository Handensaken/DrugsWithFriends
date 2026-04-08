using System;
using StateMachine.Solid;
using UnityEngine;

namespace StateMachineScripts
{
    public class AttackAnimationEventHandler : MonoBehaviour,IAnimationEvent
    {
        public event Action OnAnimationEvent;

        public void AnimationEvent()
        {
            OnAnimationEvent?.Invoke();
        }
    }
}