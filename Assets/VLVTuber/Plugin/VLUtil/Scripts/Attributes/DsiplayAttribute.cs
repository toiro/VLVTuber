using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLUtil
{
    public class DisplayAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisplayAttribute))]
    public class DisplayDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {

            EditorGUI.BeginProperty(position, label, prop);

            prop.intValue = EditorGUI.IntPopup(position, label, prop.intValue, DisplayUtility.GetDisplayNames(), DisplayUtility.GetDisplayIndices());

            EditorGUI.EndProperty();
        }

        // UnityCsReference/Editor/Mono/DisplayUtility.cs
        // 公開されていないのでコピペ……
        class DisplayUtility
        {
            static string s_DisplayStr = "Display {0}";
            private static GUIContent[] s_GenericDisplayNames =
            {
            TextContent(string.Format(s_DisplayStr, 1)), TextContent(string.Format(s_DisplayStr, 2)),
            TextContent(string.Format(s_DisplayStr, 3)), TextContent(string.Format(s_DisplayStr, 4)),
            TextContent(string.Format(s_DisplayStr, 5)), TextContent(string.Format(s_DisplayStr, 6)),
            TextContent(string.Format(s_DisplayStr, 7)), TextContent(string.Format(s_DisplayStr, 8))
        };

            private static readonly int[] s_DisplayIndices = { 0, 1, 2, 3, 4, 5, 6, 7 };

            public static GUIContent[] GetGenericDisplayNames()
            {
                return s_GenericDisplayNames;
            }

            public static int[] GetDisplayIndices()
            {
                return s_DisplayIndices;
            }

            public static GUIContent[] GetDisplayNames()
            {
                //プラットホーム対応はとりあえずあきらめる
                //var a = typeof(EditorGUIUtility).GetMethod("TextContent", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "UnityEditor.InspectorWindow" }) as GUIContent;

                //GUIContent[] platformDisplayNames = Modules.ModuleManager.GetDisplayNames(EditorUserBuildSettings.activeBuildTarget.ToString());
                //return platformDisplayNames != null ? platformDisplayNames : s_GenericDisplayNames;
                return s_GenericDisplayNames;
            }

            // ここも公開されていないので無理やり
            static GUIContent TextContent(string content) => typeof(EditorGUIUtility).GetMethod("TextContent", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { content }) as GUIContent;
        }
    }
#endif
}