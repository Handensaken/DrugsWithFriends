using Paket.StateMachineScripts.Targets;

namespace StateMachineScripts.Targets
{
    public interface ITargetProvider
    {
        public IEnemyTarget[] GetAllTargets { get; }
        
    }
}