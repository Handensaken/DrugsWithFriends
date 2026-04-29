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
            Vector3 movementDirection = agent.velocity.normalized;
            
            float proportionalForwardToMovementDirection = Vector3.Dot(movementDirection, forward);
            
            Vector3 right = agent.transform.right;
            float rightOrLeftValue = Vector3.Dot(right, movementDirection);

            //Mapping to certain values: Most right = 1.0
            if (rightOrLeftValue >= 0)
            {
                
            }
            else //Mapping to certain values: Most left = 0.0
            {
                
            }
            
            
            Debug.Log($"Movement:Forward - Fram - {Vector3.Dot(Vector3.forward, Vector3.forward)}" +
                      $"\n Back - {Vector3.Dot(Vector3.forward, -Vector3.forward)}" +
                      $"\n Rätvinklig - {Vector3.Dot(Vector3.forward, Vector3.right)}"+
                      $"\n Rätvinklig - {Vector3.Dot(Vector3.forward, Vector3.left)}" +
                      $"\n Delvis - {Vector3.Dot(Vector3.forward, (Vector3.left+Vector3.forward+Vector3.forward).normalized)}"+
                      $"\n Delvis - {Vector3.Dot(Vector3.forward, (Vector3.right+Vector3.forward).normalized)}");
            //animator.SetFloat(reciprocateParameterName, agent.velocity.magnitude);
        }
    }
}
