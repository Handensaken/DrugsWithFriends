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
    [SerializeField] public MovementParameters movementParameters;

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

[Serializable]
public struct MovementParameters
{
    [SerializeField] private float speed;
    [SerializeField] private float stoppingDistance;

    public float Speed => speed;
    public float StoppingDistance => stoppingDistance;
}

