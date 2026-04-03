using System;

namespace StateMachine.Solid
{
    public interface IAnimationEvent
    {
        public event Action OnAnimationEvent;
    }
}