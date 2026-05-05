using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct HealthValuePackage
    {
        [SerializeField] public AnimationCurve curve;
        [SerializeField,
         Tooltip("Parameter acts as a startingPoint for the curve" +
                 "\n Ex: for distance - 0 is a suitable startValue")]
        public float startValue;
        [SerializeField] public float weight;
    }
}