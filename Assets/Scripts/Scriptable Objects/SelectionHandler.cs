using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu (fileName = "SelectionHandler", menuName = "SelectionHandler")]
public class SelectionHandler : ScriptableObject
{
    public List<GameObject> selectedObjects = new List<GameObject>();
}