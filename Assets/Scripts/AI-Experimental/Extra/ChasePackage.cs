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
        [SerializeField] public MovementPackage movementPackage;
    }
}