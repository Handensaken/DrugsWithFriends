using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class InspectorButtonAttribute : PropertyAttribute
{
    public string MethodName { get; private set; }
    public string ButtonLabel { get; private set; }

    public InspectorButtonAttribute(string methodName, string buttonLabel = null)
    {
        MethodName = methodName;
        ButtonLabel = buttonLabel ?? methodName;
    }
}