using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct MovementParameters
    {
        [SerializeField] private float speed;
        [SerializeField] private float stoppingDistance;

        public float Speed => speed;
        public float StoppingDistance => stoppingDistance;
    }
}