using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct ValuePackageStart
    {
        [SerializeField] public AnimationCurve curve;
        [SerializeField, Min(1),
         Tooltip("Parameter acts as a startingPoint for the curve" +
                 "\n Ex: for distance - 0 is a suitable startValue")]
        public float startValue;
        [SerializeField,Min(0)] public float weight;
    }
}