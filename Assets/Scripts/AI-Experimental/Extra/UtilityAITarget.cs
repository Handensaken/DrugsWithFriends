using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct UtilityAITarget
    {
        [SerializeField] public ValuePackage distance;
        [SerializeField] public ValuePackage maxHealth;
    }

    [Serializable]
    public struct ValuePackage
    {
        [SerializeField] public AnimationCurve curve;
        [SerializeField,Range(0,1)] public float weight;
    }
}