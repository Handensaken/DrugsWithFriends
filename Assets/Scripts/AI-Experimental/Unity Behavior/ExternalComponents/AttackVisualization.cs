using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior;
using UnityEngine;

namespace AI_Experimental.Unity_Behavior.ExternalComponents
{
    [Serializable]
    public class AttackVisualization : Visualization
    {
        [SerializeField] private Transform eyes;
        public void Visualize(Color gizmoColor, AttackPackage attackPackage)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(eyes.position,attackPackage.minRange);
            Gizmos.DrawWireSphere(eyes.position,attackPackage.maxRange);
        }
    }
}