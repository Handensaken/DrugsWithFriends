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
        public UnityAction<int> RemovalClientData = delegate(int clientID) {};
        
        [SerializeField, Min(1)] private uint healthPerBatch;
        [SerializeField, Range(1,10)] private uint initialMaxAmountForBatches;

        [Space, Header("CustomStartingHealth")] 
        [SerializeField, Min(1)] public uint health;
        [SerializeField, Range(1,10)] public uint batchAmount;
        
        public uint HealthPerBatch => healthPerBatch;
        public uint InitialMaxAmountForBatches => initialMaxAmountForBatches;
    }
}
