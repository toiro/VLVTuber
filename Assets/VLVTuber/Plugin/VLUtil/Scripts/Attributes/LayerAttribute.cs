using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLUtil
{
    public class LayerAttribute : PropertyAttribute
    {
        // NOTHING - just oxygen.
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeEditor : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // One line of  oxygen free code.
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
#endif
}
