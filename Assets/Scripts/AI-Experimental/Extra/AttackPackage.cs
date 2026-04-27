using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct AttackPackage 
    {
        [SerializeField] private string stateName;
        [SerializeField] private Color stateColor;
        [SerializeField, Min(.1f)] public float minRange;
        [SerializeField, Min(.1f)] public float maxRange;
        
        public Color StateColor => stateColor;
    }
}