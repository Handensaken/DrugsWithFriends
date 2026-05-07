using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    [CreateAssetMenu(menuName = "Health System/HealthSO")]
    public class HealthRuleData : ScriptableObject
    {
        //TODO NewHealthUnit-event (new player registered)
        public UnityAction<int> RequestHealth = delegate (int clientID){};
        public UnityAction<int,HealthPackage> UpdateHealth = delegate(int clientID,HealthPackage healthPackage) {};
        public UnityAction<int> RemovalOfClientData = delegate(int clientID) {};
        
        [SerializeField, Min(1)] private uint healthPerBatch;
        [SerializeField, Range(1,10)] private uint initialMaxAmountForBatches;
        
        public uint HealthPerBatch => healthPerBatch;
        public uint InitialMaxAmountForBatches => initialMaxAmountForBatches;
    }
}
