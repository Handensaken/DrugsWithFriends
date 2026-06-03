using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct ChasePackage
    {
        [SerializeField] private string stateName;
        [SerializeField] public Color stateColor;
        
        [Space,Header("Movement")]
        [SerializeField] public ValuePackage speedValuePackage;
        [SerializeField, Min(.1f)] private float acceleration;
        [SerializeField, Min(.1f)] private float stoppingDistance;

        [Space, SerializeField] public float closeToPlayerRange;
        
        public float Acceleration => acceleration;
        public float StoppingDistance => stoppingDistance;
    }
}