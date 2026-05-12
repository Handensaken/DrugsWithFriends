using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public abstract class BattleCircleAi
    {
        protected void SetAITransformPoint(BlackboardReference blackboard, Transform targetTransform)
        {
            blackboard.SetVariableValue("Target",targetTransform);
        }
    }
}