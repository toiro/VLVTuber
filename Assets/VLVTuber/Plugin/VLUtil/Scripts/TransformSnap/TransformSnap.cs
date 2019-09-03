using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLUtil
{
    [CreateAssetMenu(menuName = "VacantLot/TransformSnap")]
    public class TransformSnap : ScriptableObject
    {
        [SerializeField]
        public List<TransformSnapElement> Values;

        public void Apply(Transform root)
        {
            foreach (TransformSnapElement e in Values)
            {
                var t = (e.Path.Length == 0) ? root : root.Find(e.Path);
                e.Value.Apply(t);
            }
        }

        public static TransformSnap Snap(Transform root)
        {
            var asset = ScriptableObject.CreateInstance<TransformSnap>();
            asset.Values = root.Decendants().Select(_ => new TransformSnapElement(_, root)).ToList();
            return asset;
        }

        public static TransformSnap Lerp(TransformSnap a, TransformSnap b, float t)
        {
            // a and b must have same set of transform paths
            var asset = ScriptableObject.CreateInstance<TransformSnap>();
            asset.Values = a.Values.Select(
                _a => new TransformSnapElement(
                    _a.Path,
                    TransformValue.LerpSlerpLerp(_a.Value, b.Values.Find(_b => _a.Path == _b.Path).Value, t))
                    ).ToList();
            return asset;
        }
    }

    [Serializable]
    public struct TransformSnapElement
    {
        public TransformSnapElement(Transform self, Transform root)
        {
            Path = GetHierarchyPath(self, root);
            Value = new TransformValue(self);
        }

        public TransformSnapElement(string path, TransformValue value)
        {
            this.Path = path;
            this.Value = value;
        }

        public string Path;
        public TransformValue Value;

        private static string GetHierarchyPath(Transform self, Transform root)
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
