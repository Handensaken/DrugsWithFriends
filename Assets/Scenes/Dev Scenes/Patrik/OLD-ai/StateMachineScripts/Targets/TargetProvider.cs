using System;
using System.Linq;
using FishNet.Object;
using Paket.StateMachineScripts.Targets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StateMachineScripts.Targets
{
    public class TargetProvider : ITargetProvider
    {
        protected IEnemyTarget[] AllTargets;
        
        public IEnemyTarget[] GetAllTargets
        {
            get
            {
                AllTargets = FindAllTargets();
                return SortOnlyActive();
            }
        }

        protected IEnemyTarget[] FindAllTargets()
        {
            return Object.FindObjectsByType<NetworkBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).OfType<IEnemyTarget>().ToArray();
        }

        private IEnemyTarget[] SortOnlyActive()
        {
            return Array.FindAll(AllTargets, target => target.IsActive);
        }
    }

    public class CertifiedTargetProvider : TargetProvider, ICertifiedTargetProvider
    {
        private readonly Func<IEnemyTarget,bool> condition;

        public CertifiedTargetProvider(Func<IEnemyTarget, bool> condition)
        {
            this.condition = condition;
        }
        
        public IEnemyTarget[] GetCertainTargets()
        {
            Debug.Log("find certain");
            AllTargets ??= FindAllTargets();

            return AllTargets.Where(condition).Where(target => target.IsActive).ToArray();
        }
    }
}