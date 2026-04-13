using System;
using UnityEngine;
using UnityEngine.Serialization;

//This type of setup might be better for FSM, but it works.
[CreateAssetMenu(menuName = "AI/Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] public StateParameters stateParameters;
}

[Serializable]
public struct StateParameters
{
    [SerializeField] private string stateName;
    [SerializeField] private Color stateColor;
    [SerializeField] public SightParameters sightParameters;

    public Color StateColor => stateColor;
}
    
[Serializable]
public struct SightParameters
{
    [SerializeField] private float range;
    [SerializeField] private float angle;

    public float Range => range;
    public float Angle => angle;
}

