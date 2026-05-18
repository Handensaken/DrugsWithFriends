using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class EditorButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InspectorButtonAttribute buttonAttr = (InspectorButtonAttribute)attribute;

        if (GUI.Button(position, buttonAttr.ButtonLabel))
        {
            object target = property.serializedObject.targetObject;
            MethodInfo method = target.GetType().GetMethod(
                buttonAttr.MethodName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method != null)
                method.Invoke(target, null);
            else
                Debug.LogWarning($"InspectorButton: No method '{buttonAttr.MethodName}' found on {target.GetType().Name}");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}