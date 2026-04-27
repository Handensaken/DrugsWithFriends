using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct SightParameters
    {
        [SerializeField, Min(.1f)] private float fovRange;
        [SerializeField, Min(.1f)] private float fovAngle;

        [SerializeField, Min(.1f)] private float instantInRange;

        public float FOVRange => fovRange;
        public float FOVAngle => fovAngle;
        public float InstantInRange => instantInRange;
    }
}