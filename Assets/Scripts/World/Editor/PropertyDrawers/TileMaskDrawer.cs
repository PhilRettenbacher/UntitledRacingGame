using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TileMask))]
public class TileMaskDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);


            float currentHeight = position.y + EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    var m_maskX = property.FindPropertyRelative("maskX");
                    var m_grid = property.FindPropertyRelative("serializableGrid");
                    var m_entryPosition = property.FindPropertyRelative("entryPosition");
                    var m_exitPosition = property.FindPropertyRelative("exitPosition");

                    float entryPositionHeigth = EditorGUI.GetPropertyHeight(m_entryPosition);
                    Rect entryPositionRect = new Rect(position.x, currentHeight, position.width, entryPositionHeigth);
                    EditorGUI.PropertyField(entryPositionRect, m_entryPosition);
                    currentHeight += entryPositionHeigth;

                    float exitPositionHeigth = EditorGUI.GetPropertyHeight(m_exitPosition);
                    Rect exitPositionRect = new Rect(position.x, currentHeight, position.width, exitPositionHeigth);
                    EditorGUI.PropertyField(exitPositionRect, m_exitPosition);
                    currentHeight += exitPositionHeigth;

                    int gridSize = m_grid.arraySize;

                    int x = m_maskX.intValue;
                    int y = x != 0 ? gridSize / x : 1;
                    EditorGUI.BeginChangeCheck();
                    Rect dimensionsRect = new Rect(position.x, currentHeight, position.width, EditorGUIUtility.singleLineHeight);
                    Vector2Int dimensions = EditorGUI.Vector2IntField(dimensionsRect, new GUIContent("Dimensions"), new Vector2Int(x, y));

                    if (EditorGUI.EndChangeCheck())
                    {
                        dimensions.y = Mathf.Max(1, dimensions.y);
                        m_maskX.intValue = Mathf.Max(1, dimensions.x);
                        m_grid.ClearArray();
                        m_grid.arraySize = dimensions.x * dimensions.y;
                    }

                    currentHeight += EditorGUIUtility.singleLineHeight;

                    Rect buttonRect = new Rect(position.x, currentHeight, position.width, EditorGUIUtility.singleLineHeight);

                    if(GUI.Button(buttonRect, "Select All"))
                    {
                        for (int i = 0; i < dimensions.y; i++)
                        {
                            for (int j = 0; j < dimensions.x; j++)
                            {
                                int idx = j + dimensions.x * i;

                                m_grid.GetArrayElementAtIndex(idx).boolValue = true;
                            }
                        }
                    }

                    for (int i = dimensions.y-1; i >= 0; i--)
                    {
                        currentHeight += EditorGUIUtility.singleLineHeight;

                        for (int j = 0; j < dimensions.x; j++)
                        {
                            int idx = j + dimensions.x * i;

                            Rect cell = new Rect(EditorGUIUtility.labelWidth + position.x + j * EditorGUIUtility.singleLineHeight, currentHeight, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                            EditorGUI.PropertyField(cell, m_grid.GetArrayElementAtIndex(idx), GUIContent.none);
                        }
                    }
                }
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float size = EditorGUIUtility.singleLineHeight * 2;

        if (property.isExpanded)
        {
            var m_maskX = property.FindPropertyRelative("maskX");
            var m_grid = property.FindPropertyRelative("serializableGrid");
            var m_entryPosition = property.FindPropertyRelative("entryPosition");
            var m_exitPosition = property.FindPropertyRelative("exitPosition");

            size += EditorGUI.GetPropertyHeight(m_entryPosition);
            size += EditorGUI.GetPropertyHeight(m_exitPosition);

            int gridSize = m_grid.arraySize;

            int x = m_maskX.intValue;
            int y = x != 0 ? gridSize / x : 0;

            size += (y + 1) * EditorGUIUtility.singleLineHeight;
        }

        return size;
    }
}
