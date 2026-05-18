using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    [CreateAssetMenu(menuName = "AI/BattleCircle/BattelCircleData")]
    public class BattleCircleData : ScriptableObject
    {
        [SerializeField,Min(0.1f)] public float circleRange;
        [SerializeField] public TokenManagingPackage tokenCreationData;
    }

    [Serializable]
    public struct TokenManagingPackage
    {
        [Space, SerializeField,Range(0,3)] public int minTime;
        [Space, SerializeField,Range(1,10)] public int maxTime;
    }
}