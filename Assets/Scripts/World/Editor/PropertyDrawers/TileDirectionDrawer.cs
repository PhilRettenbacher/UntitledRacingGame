using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TileDirection))]
public class TileDirectionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            var m_value = property.FindPropertyRelative("value");

            m_value.intValue = EditorGUI.Popup(position, label.text, m_value.intValue, TileDirection.names);

        }
    }
}
