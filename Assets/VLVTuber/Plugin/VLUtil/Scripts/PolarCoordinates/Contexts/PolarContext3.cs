using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [System.Serializable]
    public struct PolarContext3
    {
        public PolarContext3(Vector3 polar, Quaternion rotation, bool isRightHanded = false)
        {
            this.polar = polar;
            this.rotation = isRightHanded? MirrorHanded(rotation): rotation;
            this.isRightHanded = isRightHanded;
        }

        public PolarContext3(Vector3 polar, Vector3 rotation, bool isRightHanded)
            : this(polar, Quaternion.Euler(rotation.x, rotation.y, rotation.z), isRightHanded)
        { }

        public Vector3 polar;
        public Quaternion rotation;
        public bool isRightHanded;

        public static implicit operator PolarContext3(PolarContext2 c2)
            => new PolarContext3(c2.polar, new Vector3(0f, 0f, c2.rotation), !c2.isClockwise);

        private static Quaternion MirrorHanded(Quaternion q) => new Quaternion(-q.x, q.y, q.z, -q.w);

        public readonly static PolarContext3 zero = new PolarContext3(Vector3.zero, Quaternion.identity);
        public readonly static PolarContext3 vertical = new PolarContext3(Vector3.zero, Quaternion.LookRotation(Vector3.up, Vector3.left));
    }
}
