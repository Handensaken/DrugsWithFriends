using UnityEditor;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI
{
    [CustomEditor(typeof(AgentPathPoints))]
    public class AgentPathGUI : Editor
    {
        private void OnSceneGUI()
        {
            if (Application.isEditor)
            {
                AgentPathPoints behaviour = (AgentPathPoints)target;
                
                if (!Application.isPlaying)
                {
                    Vector3 startPos = behaviour.transform.position;
                    for (int i = 0; i < behaviour.LocalPatrolPoints.Length; i++)
                    {
                        EditorGUI.BeginChangeCheck();

                        Vector3 patrolHandlePosition =
                            Handles.PositionHandle(startPos + behaviour.LocalPatrolPoints[i], Quaternion.identity);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(behaviour, "Change patrolPoint's position");
                            behaviour.LocalPatrolPoints[i] = patrolHandlePosition - startPos;
                            behaviour.UpdateWorldPatrolPoints(startPos);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < behaviour.WorldCoordPatrolPoints.Length; i++)
                    {
                        EditorGUI.BeginChangeCheck();

                        Vector3 patrolHandlePosition =
                            Handles.PositionHandle(behaviour.WorldCoordPatrolPoints[i], Quaternion.identity);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(behaviour, "Change patrolPoint's position");
                            behaviour.WorldCoordPatrolPoints[i] = patrolHandlePosition;
                            behaviour.UpdateLocalPatrolPoints(behaviour.OriginalPosition);
                        }
                    }
                }
            }
        }
    }
}
