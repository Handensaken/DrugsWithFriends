using System;
using AI_Experimental.Unity_Behavior.ExternalComponents;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior;
using UnityEngine;

public class EnemyVisualization : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [Space,Header("Different visualizations"),SerializeField] private SightVisualization patrolSight = new SightVisualization();
    [SerializeField] private AttackVisualization attackRange = new AttackVisualization();

    private void OnDrawGizmosSelected()
    {
        if (patrolSight.GetTypeOfVisualization == TypeOfVisualization.Selected)
        {
            PatrolPackage patrolPackage = enemyData.patrolPackage;
            patrolSight.Visualize(patrolPackage.stateColor, patrolPackage.sightPackage);
        }
        
        if (attackRange.GetTypeOfVisualization == TypeOfVisualization.Selected)
        {
            AttackPackage attackPackage = enemyData.attackPackage;
            attackRange.Visualize(attackPackage.stateColor, attackPackage);
        }
    }

    private void OnDrawGizmos()
    {
        if (patrolSight.GetTypeOfVisualization == TypeOfVisualization.Always)
        {
            PatrolPackage patrolPackage = enemyData.patrolPackage;
            patrolSight.Visualize(patrolPackage.stateColor, patrolPackage.sightPackage);
        }
        if (attackRange.GetTypeOfVisualization == TypeOfVisualization.Always)
        {
            AttackPackage attackPackage = enemyData.attackPackage;
            attackRange.Visualize(attackPackage.stateColor, attackPackage);
        }
    }
}