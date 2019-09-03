using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VacantLot.VLUtil
{
    /// <summary>
    /// Rotation をエディタ上で Euler 角表示させる Attribute
    /// </summary>
    public class EulerRotationAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EulerRotationAttribute))]
    class EulerRotationAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GetType(property) == typeof(Quaternion))
                RotationPropertyField(position, property);
            else
                EditorGUI.PropertyField(position, property, label);
        }

        void RotationPropertyField(Rect position, SerializedProperty rotationProperty)
        {
            Quaternion Rotation = new Quaternion(
                rotationProperty.FindPropertyRelative("x").floatValue,
                rotationProperty.FindPropertyRelative("y").floatValue,
                rotationProperty.FindPropertyRelative("z").floatValue,
                rotationProperty.FindPropertyRelative("w").floatValue
                );

            EditorGUI.BeginChangeCheck();

            // Quaternion への変換で情報が失われるので Transform の Rotation ほど自在の値にはならない
            Vector3 eulerAngles = EditorGUI.Vector3Field(position, rotationProperty.name, Rotation.eulerAngles);

            if (EditorGUI.EndChangeCheck())
            {
                Quaternion newRotation = Quaternion.Euler(eulerAngles);
                rotationProperty.FindPropertyRelative("x").floatValue = newRotation.x;
                rotationProperty.FindPropertyRelative("y").floatValue = newRotation.y;
                rotationProperty.FindPropertyRelative("z").floatValue = newRotation.z;
                rotationProperty.FindPropertyRelative("w").floatValue = newRotation.w;
            }
        }

        public static System.Type GetType(SerializedProperty property)
        {
            System.Type parentType = property.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);
            return fi.FieldType;
        }
    }
#endif
}
