using Scenes.Dev_Scenes.Patrik.Health_system;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class HealthBatch : MonoBehaviour
    {
        [SerializeField] private RectTransform batchRect;
        [SerializeField] private RectTransform healthRect;

        [Space] [SerializeField] private HealthSO healthData;

        public RectTransform BatchRect => batchRect;
        public RectTransform HealthRect => healthRect;
        
        
    }
}
