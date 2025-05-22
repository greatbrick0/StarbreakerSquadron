using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisplayAttribute))]
public class DisplayAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        switch (property.propertyType)
        {
            default:
            case SerializedPropertyType.String:
                EditorGUI.LabelField(position, label, new GUIContent(property.stringValue));
                break;
            case SerializedPropertyType.Integer:
                EditorGUI.LabelField(position, label, new GUIContent(property.intValue.ToString()));
                break;
            case SerializedPropertyType.Boolean:
                EditorGUI.LabelField(position, label, new GUIContent(property.boolValue.ToString()));
                break;
            case SerializedPropertyType.Float:
                EditorGUI.LabelField(position, label, new GUIContent(property.floatValue.ToString()));
                break;
            case SerializedPropertyType.Vector2:
                EditorGUI.LabelField(position, label, new GUIContent(property.vector2Value.ToString()));
                break;
            case SerializedPropertyType.Vector3:
                EditorGUI.LabelField(position, label, new GUIContent(property.vector3Value.ToString()));
                break;
        }
        EditorGUI.EndProperty();
    }
}
