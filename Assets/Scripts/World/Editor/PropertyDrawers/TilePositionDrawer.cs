using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TilePosition))]
public class TilePositionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);

        if (property.isExpanded)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                Rect pos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                var m_position = property.FindPropertyRelative(nameof(TilePosition.position));
                var m_direction = property.FindPropertyRelative(nameof(TilePosition.direction));

                EditorGUI.PropertyField(pos, m_position);

                pos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(pos, m_direction);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float size = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            size += 2 * EditorGUIUtility.singleLineHeight;
        }

        return size;
    }
}
