using UnityEngine;
using UniGLTF;
using System.IO;
using System;
using RootMotion;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLVTuber
{
    using InstanceID = System.Int32;

    [RequireComponent(typeof(VRIKProvider))]
    public class VRIKProviderConsole : MonoBehaviour
    {
        public VRIKProvider Provider => GetComponent<VRIKProvider>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VRIKProviderConsole))]
    public partial class VRIKProviderConsoleEditor : Editor
    {
        bool IsActive => Application.isPlaying;

        VRIKProviderConsole Console => target as VRIKProviderConsole;
        VRIKProvider Provider => (target as VRIKProviderConsole).Provider;
        VRIKProvider.AdjustParameterSet ParameterSet => Provider.AdjustParameters;

        bool WillSetUp { set; get; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            WillSetUp = false;

            ToggleTrackingStyle();

            HorizontalLine();

            CaribrateRotationOffset();

            HorizontalLine();

            SaveAndLoadAdjustParameter();

            serializedObject.ApplyModifiedProperties();
            if (WillSetUp && IsActive) Provider.CallSetUp();
        }

        public override bool RequiresConstantRepaint() => IsActive;

        void HorizontalLine() => GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
    }

    partial class VRIKProviderConsoleEditor : Editor
    {
        public int UpdateOffsetWaitSecond = 3;

        #region Adjust Parameter
        // 最後に利用したディレクトリを使う
        static string _AssetDir = null;
        static string AssetDir { get { return _AssetDir ?? Application.dataPath; } set { _AssetDir = value; } }

        void SaveAndLoadAdjustParameter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Adjust Parameter"))
                {
                    var path = AdjustParameterSaveFilePanel;
                    SaveAdjustParameter(path);
                }

                if (GUILayout.Button("Load Adjust Parameter"))
                {
                    var path = AdjustParameterLoadFilePanel;
                    LoadAdjustParameter(path);
                }
            }
        }

        string AdjustParameterSaveFilePanel => EditorUtility.SaveFilePanel(
                    "Save Adjust Parameter",
                    AssetDir,
                    string.Format("Adjust_{0}.asset", Provider.name),
                    "asset");

        string AdjustParameterLoadFilePanel => EditorUtility.OpenFilePanel(
                    "Load Adjust Parameter",
                    AssetDir,
                    "asset");

        void SaveAdjustParameter(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            path = path.ToUnityRelativePath();
            var param = ScriptableObject.CreateInstance<VRIKAdjustParameter>();
            param.Value = Provider.AdjustParameters;
            AssetDatabase.CreateAsset(param, path);
            AssetDatabase.ImportAsset(path);

            AssetDatabase.SaveAssets();
            AssetDir = Path.GetDirectoryName(path);
            Debug.LogFormat("Save {0}.", path);
        }

        void LoadAdjustParameter(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            path = path.ToUnityRelativePath();
            var param = AssetDatabase.LoadAssetAtPath<VRIKAdjustParameter>(path);

            if (!param) return;

            // ファイルに変更を反映させない
            param = UnityEngine.Object.Instantiate<VRIKAdjustParameter>(param);

            Undo.RecordObject(Provider, "Load Adjust Parameter to " + Provider.name);
            Provider.AdjustParameters = param.Value;
            AssetDir = Path.GetDirectoryName(path);
            Debug.LogFormat("Load {0}.", path);

            WillSetUp = true;
        }
        #endregion

        void ToggleTrackingStyle()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();

                GUILayout.Label("Tracking Style");
                var useHmd = GUILayout.Toggle(Provider.Style.UseHMD, "UseHMD", EditorStyles.toggle);
                var trackPelvis = GUILayout.Toggle(Provider.Style.TrackPelvis, "TrackPelvis", EditorStyles.toggle);
                var trackFeet = GUILayout.Toggle(Provider.Style.TrackFeet, "TrackFeet", EditorStyles.toggle);


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Provider, "Change Style of " + Provider.name);

                    Provider.Style.UseHMD = useHmd;
                    Provider.Style.TrackPelvis = trackPelvis;
                    Provider.Style.TrackFeet = trackFeet;

                    WillSetUp = true;
                }
            }
        }

        #region Calibrate Offset
        void CaribrateRotationOffset()
        {
            EditorGUILayout.LabelField("Caribrate Rotation Offset");

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.IntSlider("Wait(Sec)", UpdateOffsetWaitSecond, 1, 10);

                if (check.changed)
                {
                    Undo.RecordObject(this, "Change VRIKProviderConsole");
                    UpdateOffsetWaitSecond = newValue;
                }
            }

            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledGroupScope(!Provider.Style.UseHMD))
            {
                DrawOffsetCalibration(VRIKProvider.TrackingTarget.Head, true);
            }

            using (new EditorGUI.DisabledGroupScope(Provider.Style.UseHMD))
            {
                DrawOffsetCalibration(VRIKProvider.TrackingTarget.Head);
            }

            using (new EditorGUI.DisabledGroupScope(!Provider.Style.TrackPelvis))
            {
                DrawOffsetCalibration(VRIKProvider.TrackingTarget.Pelvis);
            }

            DrawOffsetCalibration(VRIKProvider.TrackingTarget.LeftHand);
            DrawOffsetCalibration(VRIKProvider.TrackingTarget.RightHand);

            using (new EditorGUI.DisabledGroupScope(!Provider.Style.TrackFeet))
            {
                DrawOffsetCalibration(VRIKProvider.TrackingTarget.LeftLeg);
                DrawOffsetCalibration(VRIKProvider.TrackingTarget.RightLeg);
            }

            EditorGUI.indentLevel--;
        }

        void DrawOffsetCalibration(VRIKProvider.TrackingTarget target, bool useHMD = false)
        {
            var transform = Provider.TrackingTargets.Get(target);
            if (!transform) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                Quaternion offsetRotation = Provider.CalcOffsetRotation(target, useHMD);

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.Vector3Field(transform.name + " Rotation Offset", offsetRotation.eulerAngles);
                }
                using (new EditorGUI.DisabledGroupScope(!IsActive))
                {
                    if (GUILayout.Button("Set Offset"))
                    {
                        Provider.UpdateRotationOffset(target, UpdateOffsetWaitSecond, useHMD);
                    }
                }
            }
        }


        #endregion
    }


#endif
}
