using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
    public interface IVisualization
    {
        public void Visualize(Color color, ParameterPackage parameterPackage);
    
    }

    public struct ParameterPackage
    {
        public SightPackage sightPackage;
    }
}