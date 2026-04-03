using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;

namespace StateMachine.Solid
{
    public interface ITargetFilter
    {
        public bool Filter(IEnemyTarget enemyTarget);
    }
}