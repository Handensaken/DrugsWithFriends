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
    }
    
    [Serializable]
    public struct TokenManagingPackage
    {
        [Space, SerializeField,Range(0,3)] public int minTime;
        [Space, SerializeField,Range(1,10)] public int maxTime;
    }
}