using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior;
using UnityEngine;

namespace AI_Experimental.Unity_Behavior.ExternalComponents
{
    [Serializable]
    public class AttackVisualization : Visualization
    {
        [SerializeField] private ExternalSight sight;
        public void Visualize(Color gizmoColor, AttackPackage attackPackage)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(sight.eyes.position,attackPackage.minRange);
            Gizmos.DrawWireSphere(sight.eyes.position,attackPackage.maxRange);
        }
    }
}