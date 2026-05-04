using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        [SerializeField,
         Tooltip("Parameter acts as a startingPoint for the curve" +
                 "\n Ex: for distance - 0 is a suitable startValue")]
        public float startValue;
        
        [SerializeField,
         Tooltip("Parameter acts as an endPoint for the curve" +
                 "\n Ex: for distance - 20 is a suitable endValue")]
        public float endValue;
        
        [SerializeField] public float weight;
    }
}