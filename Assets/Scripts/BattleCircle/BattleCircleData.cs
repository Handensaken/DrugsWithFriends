using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    [CreateAssetMenu(menuName = "AI/BattleCircle/BattelCircleData")]
    public class BattleCircleData : ScriptableObject
    {
        [SerializeField,Min(0.1f)] public float circleRadius;
        [SerializeField, Min(1)] public uint amountOfPointsInCircle;
        
        [Space, SerializeField] public ValuePackage forwardPriorityPackage;
        
        [Space,SerializeField] public TokenManagingPackage attackTokenCreationData;
        [Space,SerializeField] public TokenManagingPackage tauntTokenCreationData;


        private void OnValidate()
        {
            //Attack
            if (attackTokenCreationData.minTime > attackTokenCreationData.maxTime)
            {
                attackTokenCreationData.minTime = attackTokenCreationData.maxTime;
            }
            
            //Taunt
            if (tauntTokenCreationData.minTime > tauntTokenCreationData.maxTime)
            {
                tauntTokenCreationData.minTime = tauntTokenCreationData.maxTime;
            }
        }
    }
    
    
}