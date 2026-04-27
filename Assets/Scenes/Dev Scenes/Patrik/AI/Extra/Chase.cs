using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct Chase
    {
        [SerializeField] private string stateName;
        [SerializeField] private Color stateColor;
        [SerializeField] public MovementParameters movementParameters;

        public Color StateColor => stateColor;
    }
}