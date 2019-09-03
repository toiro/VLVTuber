using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLUtil
{
    /// <summary>
    /// インスペクタにシーンをDnDするためのプロキシーオブジェクト
    /// Build 対象に入っているシーンのみ対応
    /// </summary>
    [System.Serializable]
    public class SceneObject
    {
        [SerializeField]
        string SceneName;

        // enable to cast between string
        public static implicit operator string(SceneObject sceneObject) => sceneObject?.SceneName;
        public static implicit operator SceneObject(string sceneName) => string.IsNullOrEmpty(sceneName) ? null : new SceneObject() { SceneName = sceneName };
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneObject))]
    public class SceneObjectEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currentValueProperty = property.FindPropertyRelative("SceneName");
            var newValue = EditorGUI.ObjectField(position, label, GetSceneAsset(currentValueProperty.stringValue), typeof(SceneAsset), false);

            if (newValue == null)
            {
                currentValueProperty.stringValue = "";
            }
            else if (newValue.name != currentValueProperty.stringValue)
            {
                if (GetSceneAsset(newValue.name))
                {
                    currentValueProperty.stringValue = newValue.name;
                }
                else
                {
                    //don't apply
                    Debug.LogWarning("Scene [" + newValue.name + "] cannot be used. Add this scene to the 'Scenes in the Build' in the build settings.");
                }
            }
        }

        static SceneAsset GetSceneAsset(string sceneObjectName)
        {
            var buildSettingScene = EditorBuildSettings.scenes.FirstOrDefault(_ => IsMatcheSceneName(_.path, sceneObjectName));

            if (buildSettingScene == null)
            {
                if (!string.IsNullOrEmpty(sceneObjectName))
                {
                    Debug.Log("Scene [" + sceneObjectName + "] cannot be used. Add this scene to the 'Scenes in the Build' in the build settings.");
                }
                return null;
            }

            return AssetDatabase.LoadAssetAtPath(buildSettingScene.path, typeof(SceneAsset)) as SceneAsset;
        }

        static bool IsMatcheSceneName(string path, string name)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name)) return false;

            return path.Split('/').Last().Equals(name + ".unity");
        }
    }
#endif
}
