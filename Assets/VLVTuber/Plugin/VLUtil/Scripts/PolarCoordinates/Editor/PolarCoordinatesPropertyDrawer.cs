using UnityEditor;
using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    public abstract class PolarCoordinatesPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldRect = EditorGUI.IndentedRect(position);
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                fieldRect = EditorGUI.PrefixLabel(fieldRect, label);

                var indentBuf = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var labelWidthBuf = EditorGUIUtility.labelWidth;

                DrawFields(fieldRect, property);

                EditorGUI.indentLevel = indentBuf;
                EditorGUIUtility.labelWidth = labelWidthBuf;
            }
        }

        protected abstract void DrawFields(Rect fieldRect, SerializedProperty property);
    }
}

