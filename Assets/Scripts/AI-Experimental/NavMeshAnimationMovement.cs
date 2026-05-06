using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik._4Animations
{
    //TODO inkludera hastigheten också för att få en uppfattning av styrka
    //TODO utveckla med struct & generic types
    public class NavMeshAnimationMovement : MonoBehaviour
    {
        [SerializeField] private Parameter reciprocateParameter;
        [SerializeField] private Parameter strafingParameter;
        
        [Space,Header("References"),SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyData enemyData;

        private void Update()
        {
            HandleMovementValues();
        }

        private void HandleMovementValues()
        {
            Vector3 forward = agent.transform.forward;
            Vector3 right = agent.transform.right;
            Vector3 velocityDirection = agent.velocity.normalized;
            
            float dotVelocityAndForward = Vector3.Dot(velocityDirection, forward);
            HandleReciprocateValue(dotVelocityAndForward);
            HandleStrafingValue(dotVelocityAndForward, velocityDirection, right);
        }

        private void HandleReciprocateValue(float dotVelocityAndForward)
        {
            float reciprocateValue = MappingValue(dotVelocityAndForward, reciprocateParameter);
            animator.SetFloat(reciprocateParameter.animationParameterName, reciprocateValue);
        }
        
        private void HandleStrafingValue(float dotVelocityAndForward, Vector3 velocityDirection, Vector3 right)
        {
            float absDotConvertedToStrafing = 1-Mathf.Abs(dotVelocityAndForward);
            float strafingValue;
            float leftOrRightDot = Vector3.Dot(velocityDirection, right);
            
            if (leftOrRightDot > 0) //Right
            {
                strafingValue = absDotConvertedToStrafing;
            }
            else if (leftOrRightDot < 0) //Left
            {
                strafingValue = -absDotConvertedToStrafing;
            }
            else
            {
                //Velocity is only backward or forward, neither left nor right => 0% strafing
                strafingValue = 0;
            }

            strafingValue = MappingValue(strafingValue, strafingParameter);
            animator.SetFloat(strafingParameter.animationParameterName, strafingValue);
        }
        
        private float MappingValue(float value, Parameter parameter)
        {
            return parameter.baseValue + parameter.maxValueChange * value;
        }
        
        [Serializable]
        private struct Parameter
        {
            public string animationParameterName;
            public float baseValue;
            public float maxValueChange;
        }
    }
}
