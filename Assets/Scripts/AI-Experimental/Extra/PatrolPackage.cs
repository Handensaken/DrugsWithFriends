using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct PatrolPackage
    {
        [SerializeField] private string stateName;
        [SerializeField] private Color stateColor;
        [SerializeField] public SightPackage sightPackage;
        [SerializeField] public MovementPackage movementPackage;

        public Color StateColor => stateColor;
    }
}