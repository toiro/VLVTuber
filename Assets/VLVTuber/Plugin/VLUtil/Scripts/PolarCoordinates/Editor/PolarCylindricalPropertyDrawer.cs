using UnityEditor;
using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [CustomPropertyDrawer(typeof(PolarCylindrical))]
    public class PolarCylindricalPropertyDrawer : PolarCoordinatesPropertyDrawer
    {
        protected override void DrawFields(Rect fieldRect, SerializedProperty property)
        {
            EditorGUIUtility.labelWidth = 15;
            var rRect = fieldRect;
            rRect.width /= 3f;
            EditorGUI.PropertyField(rRect, property.FindPropertyRelative("r"));

            EditorGUIUtility.labelWidth = 15;
            var zRect = fieldRect;
            zRect.width /= 3f;
            zRect.x += rRect.width;
            EditorGUI.PropertyField(zRect, property.FindPropertyRelative("z"));

            EditorGUIUtility.labelWidth = 40;
            var thetaRect = fieldRect;
            thetaRect.width -= rRect.width;
            thetaRect.x += rRect.width;
            thetaRect.width -= zRect.width;
            thetaRect.x += zRect.width;
            EditorGUI.PropertyField(thetaRect, property.FindPropertyRelative("theta"));
        }
    }
}