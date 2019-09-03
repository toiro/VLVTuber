using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLUtil
{
    /// <summary>
    /// マルチディスプレイを有効にする。
    /// </summary>
    public class MultiDisplayManager : MonoBehaviour
    {
        [SerializeField]
        [Range(1, 8)]
        int RequiredDisplayCount = 1;

        public int ActivatedCount { get; private set; }

        // Use this for initialization
        void Start()
        {
            ActivatedCount = 1;
            var availableCount = System.Math.Min(RequiredDisplayCount, Display.displays.Length);

            if (availableCount == 1) return;

            if (availableCount < RequiredDisplayCount)
            {
                Debug.LogFormat("{0} Displays Required but only {1} Available.", RequiredDisplayCount, availableCount);
            }

            // ディスプレイを有効化する
            for (int i = 1; i < availableCount; i++)
            {
                Display.displays[i].Activate();
            }
            ActivatedCount = availableCount;

            Debug.LogFormat("{0} Displays Activated.", ActivatedCount);
        }

        public void SwapDisplay(int displayX, int displayY) => SwapDisplayRawIndex(displayX - 1, displayY - 1);

        public void SwapDisplayRawIndex(int rawDisplayX, int rawDisplayY)
        {
            // Displayの表示名の番号は1スタートだが、内部インデックスは0スタート
            Camera.allCameras.Where(_ => _.targetDisplay == rawDisplayX || _.targetDisplay == rawDisplayY).OrderBy(_ => -_.depth).GroupBy(_ => _.targetDisplay).Select(_ => _.First())
                .ToList().ForEach(camera =>
                {
                    camera.targetDisplay = camera.targetDisplay == rawDisplayX ? rawDisplayY : rawDisplayX;
                });
        }

#if UNITY_EDITOR
        /// <summary>
        /// ディスプレイをターゲットするカメラを一覧表示させるエディタ拡張。
        /// </summary>
        [CustomEditor(typeof(MultiDisplayManager))]
        public class MultiDisplayManagerEditor : Editor
        {
            MultiDisplayManager _target { get { return (MultiDisplayManager)target; } }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                //horizontal line
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                EditorGUILayout.LabelField("Cameras' Target Display");

                EditorGUI.indentLevel += 1;

                Camera.allCameras.OrderBy(_ => -_.depth).GroupBy(_ => _.targetDisplay).OrderBy(_ => _.Key)
                    .ToList().ForEach(group =>
                {
                    EditorGUILayout.LabelField("Display " + (group.Key + 1) + ":");
                    EditorGUI.indentLevel += 1;
                    EditorGUI.BeginDisabledGroup(true);
                    foreach (var camera in group)
                    {
                        EditorGUILayout.ObjectField(camera, typeof(Camera), true);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel -= 1;
                });

                EditorGUI.indentLevel -= 1;
            }
        }
#endif
    }
}

