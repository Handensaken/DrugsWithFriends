using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "ControlSchemeEvent", menuName = "Events/ControlSchemeEvent")]
public class ControlSchemeEvent : ScriptableObject
{
    public string currentControlScheme;
}