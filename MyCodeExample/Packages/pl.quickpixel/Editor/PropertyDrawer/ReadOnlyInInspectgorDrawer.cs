using UnityEngine;
using UnityEditor;
using QuickPixel.Collections;

namespace QuickPixel.Editor.PropertyDrawer
{
    namespace InternalPropertyDrawers
    {
        [CustomPropertyDrawer(typeof(ReadOnlyInInspectorAttribute))]
        internal sealed class ReadOnlyInInspectorDrawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                //using var scope = new EditorGUI.PropertyScope(position,label,property);
                GUI.enabled = false;
                label = EditorGUI.BeginProperty(position, label, property);
                
                EditorGUI.PropertyField(position, property, label, true);
                
                EditorGUI.EndProperty();
                GUI.enabled = true;
            }
        }
    }
}