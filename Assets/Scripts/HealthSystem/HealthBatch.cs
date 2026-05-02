using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class HealthBatch : MonoBehaviour
    {
        [SerializeField] private RectTransform batchRect;
        [SerializeField] private RectTransform healthRect;

        [FormerlySerializedAs("healthData")] [FormerlySerializedAs("healthSo")] [Space] [SerializeField] private HealthRuleData healthRuleData;

        public RectTransform BatchRect => batchRect;
        public RectTransform HealthRect => healthRect;
        
        
    }
}
