using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VacantLot.VLUtil
{
    /// <summary>
    /// Transform のパラメータを保持する構造体
    /// </summary>
    [Serializable]
    public struct TransformValue
    {
        public Vector3 Position;
        [EulerRotation]
        public Quaternion Rotation;
        public Vector3 Scale;

        public TransformValue(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        public TransformValue(Transform origin) : this(origin.localPosition, origin.localRotation, origin.localScale) { }

        public static TransformValue identity { get { return new TransformValue(Vector3.zero, Quaternion.identity, Vector3.one); } }

        public static TransformValue LerpSlerpLerp(TransformValue a, TransformValue b, float t)
        {
            return new TransformValue(
                Vector3.Lerp(a.Position, b.Position, t),
                Quaternion.Slerp(a.Rotation, b.Rotation, t),
                Vector3.Lerp(a.Scale, b.Scale, t)
                );
        }

        public static TransformValue Average(IEnumerable<TransformValue> values)
        {
            var ret = new TransformValue(Vector3.zero, Quaternion.identity, Vector3.zero);

            var amount = 0;
            foreach (var v in values)
            {
                amount++;

                ret.Position += v.Position;
                ret.Rotation = Quaternion.Slerp(ret.Rotation, v.Rotation, 1f / amount);
                ret.Scale += v.Scale;
            }
            if (amount == 0) throw new ArgumentException("empty IEnumerable");

            ret.Position /= amount;
            ret.Scale /= amount;
            return ret;
        }
    }

    public static class TransformValueExtension
    {
        public static void Apply(this TransformValue value, Transform target)
        {
            target.localPosition = value.Position;
            target.localRotation = value.Rotation;
            target.localScale = value.Scale;
        }
    }
}