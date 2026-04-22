using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu]
public class PauseEvent : ScriptableObject
{
    [HideInInspector] public UnityEvent OnPause = new UnityEvent();

    [HideInInspector] public UnityEvent OnUnpause = new UnityEvent();
    
    [HideInInspector] public UnityEvent OnCancel = new UnityEvent();
}