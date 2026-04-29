using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik._4Animations
{
    //TODO utveckla med struct & generic types
    public class NavMeshAnimationMovement : MonoBehaviour
    {
        [SerializeField, Tooltip("Backward = 0.0, None = 0.5, Forward = 1.0")] private string reciprocateParameterName;
        [SerializeField, Tooltip("Left = 0.0, None = 0.5, Right = 1.0")] private string strafingParameterName;
        
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
            float reciprocateValue;
            if (dotVelocityAndForward > 0)
            {
                reciprocateValue = MappingValue(dotVelocityAndForward);
            }
            else if (dotVelocityAndForward < 0)
            {
                reciprocateValue = MappingValue(dotVelocityAndForward);
            }
            else
            {
                //Velocity is only right or left, neither back nor forward => 0% reciprocate
                reciprocateValue = MappingValue(0);
            }
            
            //Debug.Log(reciprocateValue);
            animator.SetFloat(reciprocateParameterName, reciprocateValue);
        }
        
        private void HandleStrafingValue(float dotVelocityAndForward, Vector3 velocityDirection, Vector3 right)
        {
            float leftOrRightDot = Vector3.Dot(velocityDirection, right);
            float strafingValue;
            if (leftOrRightDot > 0) //Right
            {
                strafingValue = MappingValue(dotVelocityAndForward);
            }
            else if (leftOrRightDot < 0) //Left
            {
                strafingValue = MappingValue(-dotVelocityAndForward);
            }
            else
            {
                //Velocity is only backward or forward, neither left nor right => 0% strafing
                strafingValue = MappingValue(0);
            }
            
            //Debug.Log(strafingValue);
            animator.SetFloat(strafingParameterName, strafingValue);
        }
        
        private float MappingValue(float value)
        {
            //TODO change 0.5 to parameters
            return .5f + .5f * value;
        }
    }
}
