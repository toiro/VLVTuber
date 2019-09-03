using UnityEditor;
using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [CustomPropertyDrawer(typeof(PolarCircular))]
    public class PolarCircularPropertyDrawer : PolarCoordinatesPropertyDrawer
    {
        protected override void DrawFields(Rect fieldRect, SerializedProperty property)
        {
            EditorGUIUtility.labelWidth = 15;
            var rRect = fieldRect;
            rRect.width /= 3f;
            EditorGUI.PropertyField(rRect, property.FindPropertyRelative("r"));

            EditorGUIUtility.labelWidth = 40;
            var thetaRect = fieldRect;
            thetaRect.width -= rRect.width;
            thetaRect.x += rRect.width;
            EditorGUI.PropertyField(thetaRect, property.FindPropertyRelative("theta"));
        }
    }
}
