using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct PatrolPackage
    {
        [SerializeField] private string stateName;
        [SerializeField] private Color stateColor;
        [SerializeField] public SightParameters sightParameters;
        [SerializeField] public MovementParameters movementParameters;

        public Color StateColor => stateColor;
    }
}