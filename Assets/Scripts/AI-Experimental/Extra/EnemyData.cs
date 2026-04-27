using System;
using System.Collections.Generic;
using Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [CreateAssetMenu(menuName = "AI/Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Behaviours and corresponding stats")]
        [SerializeField] public PatrolPackage patrolPackage;
        [SerializeField] public ChasePackage chasePackage;
        [SerializeField] public AttackPackage attackPackage;
        
        [Space, Header("Refs")]
        [SerializeField] public HealthData healthData;

        private void OnValidate()
        {
            ColorWarning();
        }

        private void ColorWarning()
        {
            if (patrolPackage.StateColor.a < 255)
            {
                Debug.Log("PatrolColor cant have alfa value lower then 255");
                patrolPackage.stateColor.a = 255;
            }
            if (chasePackage.stateColor.a < 255)
            {
                Debug.Log("chaseColor cant have alfa value lower then 255");
                chasePackage.stateColor.a = 255;
            }
            if (attackPackage.stateColor.a < 255)
            {
                Debug.Log("attackColor cant have alfa value lower then 255");
                attackPackage.stateColor.a = 255;
            }
        }
    }
}