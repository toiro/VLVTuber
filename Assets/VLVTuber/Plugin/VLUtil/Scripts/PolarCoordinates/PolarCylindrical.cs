using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [System.Serializable]
    public struct PolarCylindrical
    {
        public float r;
        public float z;
        public float theta;

        PolarCylindrical(float r, float z, float theta)
        {
            this.r = r;
            this.z = z;
            this.theta = theta;
        }

        public PolarCylindrical(PolarCircular p, float z)
        {
            this.r = p.r;
            this.z = z;
            this.theta = p.theta;
        }

        public PolarCylindrical(Vector3 p) : this(new PolarCircular(p), p.z) { }

        PolarCylindrical(Vector3 p, PolarContext2 context2) : this(new PolarCircular(p, context2), p.z) { }

        public PolarCylindrical(Vector3 p, PolarContext3 context)
            : this(Quaternion.Inverse(context.rotation) * (p - context.polar), new PolarContext2(Vector2.zero, Mathf.Acos(context.rotation.w) * 2, context.isRightHanded)) { }

        public Vector3 ToVector3()
        {
            var p2d = new PolarCircular(r, theta).ToVector2();

            return new Vector3(p2d.x, p2d.y, z);
        }

        public Vector3 ToVector3(PolarContext3 context)
        {
            var p2d = new PolarCircular(r, theta).ToVector2();
            return context.rotation * new Vector3(p2d.x, p2d.y, z) + context.polar;
        }

        public static bool operator ==(PolarCylindrical a, PolarCylindrical b) => a.Equals(b);
        public static bool operator !=(PolarCylindrical a, PolarCylindrical b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (!(obj is PolarCylindrical)) return false;
            var p = (PolarCylindrical)obj;

            return this.r == p.r && this.theta == p.theta && this.z == p.z;
        }

        public override int GetHashCode() => r.GetHashCode() & theta.GetHashCode() & z.GetHashCode();


        public static implicit operator PolarCylindrical(PolarCircular p) => new PolarCylindrical(p.r, 0f, p.theta);
        public static explicit operator PolarCircular(PolarCylindrical p) => new PolarCircular(p.r, p.theta);
    }
}
