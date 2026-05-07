using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.AI;

namespace AI_Experimental.Unity_Behavior.CustomActions
{
    public static class UtilityAIEvaluations
    {
        public static float MapValueToCurve(float currentValue, ValuePackage valuePackage) //Make to interface //TODO make use of distance
        {
            float refinedCurrentValue = currentValue-valuePackage.startValue;
            float refinedEndValue = valuePackage.endValue-valuePackage.startValue;
            float proportionalValue = refinedCurrentValue / refinedEndValue;
            
            float curveValue = valuePackage.curve.Evaluate(proportionalValue);
            
            return curveValue * valuePackage.weight;
        }
        
        public static float MapValueToCurveCustomMaxValue(uint currentValue,uint maxValue, ValuePackageStart valuePackageStart)
        {
            float refinedCurrentValue = currentValue-valuePackageStart.startValue;
            float refinedEndValue = maxValue-valuePackageStart.startValue;
            float proportionalValue = refinedCurrentValue / refinedEndValue;
            
            float curveValue = valuePackageStart.curve.Evaluate(proportionalValue);
            
            return curveValue * valuePackageStart.weight;
        }
    }
}