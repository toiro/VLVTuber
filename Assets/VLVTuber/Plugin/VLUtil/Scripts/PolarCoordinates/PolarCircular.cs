using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [System.Serializable]
    public struct PolarCircular
    {
        public float r;
        public float theta;

        public PolarCircular(float r, float theta)
        {
            this.r = r;
            this.theta = theta;
        }

        public PolarCircular(Vector2 p, float rotationOffset, bool isClockwise = false)
        {
            r = p.magnitude;
            theta = (r == 0f) ? 0f : CalcRot(p, isClockwise) - rotationOffset;
        }

        public PolarCircular(Vector2 p) : this(p, 0f) { }

        public PolarCircular(Vector2 p, PolarContext2 context) : this(p - context.polar, context.rotation, context.isClockwise) { }

        public static bool operator ==(PolarCircular a, PolarCircular b) => a.Equals(b);
        public static bool operator !=(PolarCircular a, PolarCircular b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (!(obj is PolarCircular)) return false;
            var p = (PolarCircular) obj;

            return this.r == p.r && this.theta == p.theta;
        }

        public override int GetHashCode() => r.GetHashCode() & theta.GetHashCode();

        public static PolarCircular zero = new PolarCircular(0f, 0f);

        private static float CalcRot(Vector2 v, bool isClockwise)
        {
            var ret = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            return isClockwise ? -ret : ret;
        }
    }
}
