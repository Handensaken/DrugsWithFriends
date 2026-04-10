using Paket.StateMachineScripts.Targets;
using StateMachineScripts.Targets;

namespace StateMachineScripts.States
{
    public interface ISelectTarget
    {
        public bool Select(out IEnemyTarget selectedEnemyTarget);
    }
}