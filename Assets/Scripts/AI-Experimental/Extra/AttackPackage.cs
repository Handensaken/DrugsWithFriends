using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct AttackPackage 
    {
        [SerializeField] private string stateName;
        [SerializeField] public Color stateColor;
        
        [Space,Header("AttackRange")]
        [SerializeField, Min(.1f)] public float minRange;
        [SerializeField, Min(.1f)] public float maxRange;

        [Space, SerializeField, Range(0, 100)] public int percentageOfStart;
        [SerializeField, Range(0, 100)] public int percentageOfEnd;
        [SerializeField] public AnimationClip attackAnimation;

        public void OnValidate()
        {
            if (percentageOfStart + percentageOfEnd > 100)
            {
                Debug.LogWarning("Cant have more then 100% of the animation covered");
                
            }

            if (percentageOfStart + percentageOfEnd == 100)
            {
                Debug.LogWarning("There is no time left to activate the collider within the animation");
            }
        }
    }
}