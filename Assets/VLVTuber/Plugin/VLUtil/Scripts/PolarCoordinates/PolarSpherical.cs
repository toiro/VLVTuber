using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [System.Serializable]
    public struct PolarSpherical
    {
        public float r;
        public float theta;
        public float phi;

        public PolarSpherical(float r, float theta, float phi)
        {
            this.r = r;
            this.theta = theta;
            this.phi = phi;
        }

        public PolarSpherical(Vector3 p)
        {
            r = p.magnitude;
            theta = (r == 0f) ? 0f : Mathf.Acos(p.z / r) * Mathf.Rad2Deg;
            var r2d = ((Vector2)p).magnitude;
            phi = (r2d == 0f) ? 0f : Mathf.Sign(p.y) * Mathf.Acos(p.x / r2d) * Mathf.Rad2Deg;
        }

        public PolarSpherical(Vector3 p, Quaternion rotationOffset) : this(Quaternion.Inverse(rotationOffset) * p) { }

        public PolarSpherical(Vector3 p, PolarContext3 context) : this(p - context.polar, context.rotation) { }

        public Vector3 ToVector3()
        {
            if (r == 0f) return Vector3.zero;

            var radTheta = theta * Mathf.Deg2Rad;
            var radPhi = phi * Mathf.Deg2Rad;

            var sinTheta = Mathf.Sin(radTheta);
            var cosTheta = Mathf.Cos(radTheta);
            var sinPhi = Mathf.Sin(radPhi);
            var cosPhi = Mathf.Cos(radPhi);

            return new Vector3(sinTheta * cosPhi, sinTheta * sinPhi, cosTheta) * r;
        }

        public Vector3 ToVector3(PolarContext3 context) => context.rotation * ToVector3() + context.polar;

        public static bool operator ==(PolarSpherical a, PolarSpherical b) => a.Equals(b);
        public static bool operator !=(PolarSpherical a, PolarSpherical b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (!(obj is PolarSpherical)) return false;
            var p = (PolarSpherical)obj;
            return r == p.r && theta == p.theta && phi == p.phi;
        }

        public override int GetHashCode() => r.GetHashCode() & theta.GetHashCode() & phi.GetHashCode();

        public static implicit operator PolarSpherical(PolarCircular p) => new PolarSpherical(p.r, 0f, p.theta);
        public static explicit operator PolarCircular(PolarSpherical p) => new PolarCircular(p.r * Mathf.Sin(Mathf.Deg2Rad * p.theta), p.phi);
    }
}
