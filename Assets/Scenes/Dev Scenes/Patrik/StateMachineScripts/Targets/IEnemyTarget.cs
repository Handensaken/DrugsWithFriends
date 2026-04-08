using Paket.GameInterfaces;
using UnityEngine;

namespace Paket.StateMachineScripts.Targets
{
    public interface IEnemyTarget : IDamageable
    {
        public bool IsActive {get; set; }
        public Vector3 Position { get; }
        public Collider GetCollider { get; }
        
    }
}