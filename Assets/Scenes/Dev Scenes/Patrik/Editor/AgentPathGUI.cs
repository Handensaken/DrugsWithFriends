using StateMachine.Scripts.StateMachine.Structure;
using UnityEditor;
using UnityEngine;

namespace Paket.Editor
{
    [CustomEditor(typeof(AgentPathPoints))]
    public class SolidAgentPathGUI : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            if (Application.isEditor)
            {
                AgentPathPoints behaviour = (AgentPathPoints)target;

                Vector3 startPos = Vector3.zero;
                if (!Application.isPlaying)
                {
                    startPos = behaviour.transform.position;
                }
                else
                {
                    startPos = behaviour.GetOriginPathPosition;
                }


                for (int i = 0; i < behaviour.GetPatrolPoints.Length; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    Vector3 patrolHandlePosition =
                        Handles.PositionHandle(startPos + behaviour.GetPatrolPoints[i], Quaternion.identity);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(behaviour, "Change patrolPoint's position");
                        behaviour.GetPatrolPoints[i] = patrolHandlePosition - startPos;
                    }
                }
            }
        }
    }
}
