using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class HealthBatch : MonoBehaviour
    {
        [SerializeField] private RectTransform batchRect;
        [SerializeField] private RectTransform healthRect;

        [FormerlySerializedAs("healthSo")] [Space] [SerializeField] private HealthData healthData;

        public RectTransform BatchRect => batchRect;
        public RectTransform HealthRect => healthRect;
        
        
    }
}
