using StateMachine.Scripts.StateMachine.Structure;
using UnityEditor;
using UnityEngine;

namespace Paket.Editor
{
    [CustomEditor(typeof(AgentPathPoints))]
    public class AgentPathGUI : UnityEditor.Editor
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
                    for (int i = 0; i < behaviour.GetLocalPatrolPoints.Length; i++)
                    {
                        EditorGUI.BeginChangeCheck();

                        Vector3 patrolHandlePosition =
                            Handles.PositionHandle(startPos + behaviour.GetLocalPatrolPoints[i], Quaternion.identity);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(behaviour, "Change patrolPoint's position");
                            behaviour.GetLocalPatrolPoints[i] = patrolHandlePosition - startPos;
                            behaviour.UpdateWorldPatrolPoints(startPos);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < behaviour.GetWorldPatrolPoints.Length; i++)
                    {
                        EditorGUI.BeginChangeCheck();

                        Vector3 patrolHandlePosition =
                            Handles.PositionHandle(behaviour.GetWorldPatrolPoints[i], Quaternion.identity);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(behaviour, "Change patrolPoint's position");
                            behaviour.GetWorldPatrolPoints[i] = patrolHandlePosition;
                            behaviour.UpdateLocalPatrolPoints(behaviour.GetOriginalPos);
                        }
                    }
                }
            }
        }
    }
}
