using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumFlagsAttribute flagSettings = (EnumFlagsAttribute)attribute;
        Enum targetEnum = (Enum)fieldInfo.GetValue(property.serializedObject.targetObject);

        string propName = flagSettings.name;
        if (string.IsNullOrEmpty(propName))
        {
            propName = ObjectNames.NicifyVariableName(property.name);
        }

        EditorGUI.BeginProperty(position, label, property);
        Enum enumNew = EditorGUI.EnumMaskPopup(position, propName, targetEnum);
        property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());
        EditorGUI.EndProperty();
    }
}