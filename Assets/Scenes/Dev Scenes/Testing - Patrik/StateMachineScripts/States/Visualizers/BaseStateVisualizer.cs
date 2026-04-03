using NUnit.Framework;
using UnityEngine;

namespace StateMachine.Solid.Scripts.States
{
    public class BaseStateVisualizer : IStateVisualizer
    {
        private readonly Color stateColor;
        protected readonly Transform SightTransform;

        public BaseStateVisualizer(Color stateColor, Transform sightTransform)
        {
            this.stateColor = stateColor;
            this.SightTransform = sightTransform;
        }
        
        public virtual void Visualize()
        {
            VisualizeStateColor();
        }
        
        private void VisualizeStateColor()
        {
            Gizmos.color = stateColor;
            Gizmos.DrawCube(SightTransform.position+Vector3.up*2, new Vector3(.5f,.5f,.5f));
        }
    }
}