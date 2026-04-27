using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct SightParameters
    {
        [SerializeField] private float fovRange;
        [SerializeField] private float fovAngle;

        [SerializeField] private float instantInRange;

        public float FOVRange => fovRange;
        public float FOVAngle => fovAngle;
        public float InstantInRange => instantInRange;
    }
}