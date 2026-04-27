using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct MovementParameters
    {
        [SerializeField, Min(.1f)] private float speed;
        [SerializeField, Min(.1f)] private float stoppingDistance;

        public float Speed => speed;
        public float StoppingDistance => stoppingDistance;
    }
}