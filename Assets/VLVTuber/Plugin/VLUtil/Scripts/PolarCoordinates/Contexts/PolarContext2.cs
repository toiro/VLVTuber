using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    [System.Serializable]
    public struct PolarContext2
    {
        public Vector2 polar;
        public float rotation;
        public bool isClockwise;

        public PolarContext2(Vector2 polar, float rotation, bool isClockwise = false)
        {
            this.polar = polar;
            this.rotation = isClockwise ? -rotation : rotation;
            this.isClockwise = isClockwise;
        }

        public readonly static PolarContext2 zero = new PolarContext2(Vector2.zero, 0f);
        public readonly static PolarContext2 clock = new PolarContext2(Vector2.zero, 90f, true);
    }
}
