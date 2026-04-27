using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior;
using UnityEngine;

public class EnemyVisualization : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private SightVisualization patrolSight = new SightVisualization();

    private void OnDrawGizmosSelected()
    {
        if (patrolSight.GetVisualization == Visualization.Selected)
        {
            PatrolPackage patrolPackage = enemyData.patrolPackage;
            patrolSight.Visualize(patrolPackage.StateColor, patrolPackage.sightPackage);
        }
    }

    private void OnDrawGizmos()
    {
        if (patrolSight.GetVisualization == Visualization.Always)
        {
            PatrolPackage patrolPackage = enemyData.patrolPackage;
            patrolSight.Visualize(patrolPackage.StateColor, patrolPackage.sightPackage);
        }
        
    }
}