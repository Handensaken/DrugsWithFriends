using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameInterfaces;
using UnityEngine;

namespace Paket.StateMachineScripts.Targets
{
    public class TargetDummy : NetworkBehaviour, IEnemyTarget
    {
        
        [SerializeField] private Collider targetCollider;
        [SerializeField] private MeshRenderer meshRenderer;

        private IHealth health;
        private readonly SyncVar<bool> isActive = new SyncVar<bool>();
        
        private const float Time = 5f;
        [SerializeField] private float currentTime;
        public bool IsActive
        {
            get => isActive.Value;
            set
            {
                HandleActivity(value);
            }
        }
        
        public Vector3 Position => transform.position;
        public Collider GetCollider => targetCollider;

        private void Awake()
        {
            HandleActivity(true);
        }
        
        private void Update()
        {
            HandleInactivePhase();
        }
        
        private void HandleActivity(bool active)
        {
            isActive.Value = active;
            meshRenderer.enabled = active;
            currentTime = Time;
        }
        
        private void HandleInactivePhase()
        {
            if (!isActive.Value) currentTime -= UnityEngine.Time.deltaTime;
            
            if (currentTime <= 0)
            {
                HandleActivity(true);
            }
        }
        
        private void OnValidate()
        {
            if (targetCollider == null)
            {
                throw new Exception("Target is missing a collider ref");
            }
        }

        public void Damage()
        {
            Debug.Log("Damage");
            
        }
    }
}