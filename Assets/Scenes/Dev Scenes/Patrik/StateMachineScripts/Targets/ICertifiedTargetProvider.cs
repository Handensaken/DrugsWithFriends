using Paket.StateMachineScripts.Targets;

namespace StateMachineScripts.Targets
{
    public interface ICertifiedTargetProvider : ITargetProvider
    {
        /// <summary>
        /// For achieved all targets in like a state or likewise without knowing how they are sorted and the condition is created with the state
        /// Example: ChaseState wants only targets inside sight to decide which to chase 
        /// </summary>
        /// <returns></returns>
        public IEnemyTarget[] GetCertainTargets();
    }
}