using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.AI;

namespace AI_Experimental.Unity_Behavior.CustomActions
{
    public static class UtilityAIEvaluations
    {
        public static float DistanceValue(float distance, DistanceValuePackage distanceDistanceValuePackage) //Make to interface //TODO make use of distance
        {
            float refinedCurrentValue = distance-distanceDistanceValuePackage.startValue;
            float refinedEndValue = distanceDistanceValuePackage.endValue-distanceDistanceValuePackage.startValue;
            float proportionalValue = refinedCurrentValue / refinedEndValue;
            
            float curveValue = distanceDistanceValuePackage.curve.Evaluate(proportionalValue);
            
            return curveValue * distanceDistanceValuePackage.weight;
        }
        
        public static float MaxBatchValue(uint currentBatchAmount,uint maxValue, HealthValuePackage healthDistanceValuePackage)
        {
            float refinedCurrentValue = currentBatchAmount-healthDistanceValuePackage.startValue;
            float refinedEndValue = maxValue-healthDistanceValuePackage.startValue;
            float proportionalValue = refinedCurrentValue / refinedEndValue;
            
            float curveValue = healthDistanceValuePackage.curve.Evaluate(proportionalValue);
            
            return curveValue * healthDistanceValuePackage.weight;
        }

        public static float CurrentHealthValue()
        {
            return 0;
        }
    }
}