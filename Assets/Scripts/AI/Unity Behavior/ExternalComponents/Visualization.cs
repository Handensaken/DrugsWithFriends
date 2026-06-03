using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
    public abstract class Visualization : IVisualization
    {
        [SerializeField, Tooltip("How the visualization should occur")] private TypeOfVisualization _typeOfVisualization;
        public TypeOfVisualization GetTypeOfVisualization => _typeOfVisualization;
    }
}