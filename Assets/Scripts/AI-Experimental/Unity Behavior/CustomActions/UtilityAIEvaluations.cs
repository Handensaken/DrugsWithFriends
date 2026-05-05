using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.AI;

namespace AI_Experimental.Unity_Behavior.CustomActions
{
    public static class UtilityAIEvaluations
    {
        public static float DistanceValue(NavMeshAgent agent,Vector3 targetPosition, ValuePackage distanceValuePackage) //Make to interface //TODO make use of distance
        {
            agent.SetDestination(targetPosition);

            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(targetPosition, path); //TODO can be heavy on performance
        
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                float currentDistance = agent.remainingDistance;
                float startValue = distanceValuePackage.startValue;
                float endValue = distanceValuePackage.endValue;
                float t = (currentDistance-startValue) / (endValue-startValue);
                float curveValue = distanceValuePackage.curve.Evaluate(t);
                Debug.Log($"Distance: {currentDistance}" +
                          $"\nt: {t}" +
                          $"\ncurveValue: {curveValue}" +
                          $"\nresult: {curveValue * distanceValuePackage.weight}");
                return curveValue * distanceValuePackage.weight;
            }

            return 0;
        }

        public static float MaxHealthValue()
        {
            return 0;
        }

        public static float CurrentHealthValue()
        {
            return 0;
        }
    }
}