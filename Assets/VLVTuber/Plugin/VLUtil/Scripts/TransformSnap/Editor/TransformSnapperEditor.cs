using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VacantLot.VLUtil
{
    [CustomEditor(typeof(TransformSnapper))]
    public class TransformSnapperEditor : Editor
    {
        TransformSnapper Snapper => target as TransformSnapper;
        Transform Root => Snapper.Root;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(Snapper.Snap == null);
            if (GUILayout.Button("Apply"))
            {
                foreach (TransformSnapElement e in Snapper.Snap.Values)
                {
                    var t = (e.Path.Length == 0) ? Root : Root.Find(e.Path);
                    if (!t)
                    {
                        Debug.LogWarning("Can't fint target for Path: " + e.Path);
                    }
                    else
                    {
                        e.Value.Apply(t);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Snap"))
            {
                var list = new List<TransformSnapElement>();

                Stack<Transform> stack = new Stack<Transform>();
                stack.Push(Root);

                while (true)
                {
                    Transform current = stack.Pop();
                    #region Process
                    var e = new TransformSnapElement();
                    e.Path = GetHierarchyPath(current, Root);
                    if (!string.IsNullOrEmpty(e.Path))
                    {
                        e.Value = new TransformValue(current);
                        list.Add(e);
                    }

                    #endregion
                    foreach (Transform child in current)
                    {
                        stack.Push(child);
                    }

                    if (stack.Count == 0)
                    {
                        break;
                    }
                }

                var assetPath = string.Format("Assets/{0}.asset", Snapper.name);

                var asset = ScriptableObject.CreateInstance<TransformSnap>();
                asset.Values = list;
                Debug.LogFormat("create asset: {0}", assetPath);
                AssetDatabase.CreateAsset(asset, assetPath);

                Selection.objects = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath(assetPath, typeof(TransformSnap)) };
            }


        }

        public string GetHierarchyPath(Transform self, Transform root)
        {
            string path = "";
            while (self != root && self != self.root)
            {
                path = "/" + self.gameObject.name + path;
                self = self.parent;
            }
            if (self != null && path.Length > 0)
            {
                path = path.Substring(1);
            }
            return path;
        }

    }
}
