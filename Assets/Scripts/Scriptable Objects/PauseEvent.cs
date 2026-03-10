using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu]
public class PauseEvent : ScriptableObject
{
    [HideInInspector] public UnityEvent<PlayerInput> OnPause = new UnityEvent<PlayerInput>();

    [HideInInspector] public UnityEvent<PlayerInput> OnUnpause = new UnityEvent<PlayerInput>();
    
    [HideInInspector] public UnityEvent<PlayerInput> OnCancel = new UnityEvent<PlayerInput>();
}