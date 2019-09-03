using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VacantLot.VLUtil.PolarCoordinates
{
    public static class PolarCoordinatesExtension
    {
        public static PolarCircular NormalizeAngle(this PolarCircular coord) => new PolarCircular(coord.r, NormalizeAngle(coord.theta));

        public static Vector2 ToVector2(this PolarCircular coord)
        {
            var rad = Mathf.Deg2Rad * coord.theta;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * coord.r;
        }

        public static Vector2 ToVector2(this PolarCircular coord, PolarContext2 context)
        {
            var theta2 = context.isClockwise ? -coord.theta : coord.theta;
            var rad = Mathf.Deg2Rad * (theta2 + context.rotation);

            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * coord.r + context.polar;
        }

        #region private
        private static float NormalizeAngle(float deg)
        {
            var ret = deg % 360f;
            return ret > 0f ? ret : ret + 360f;
        }
        #endregion
    }
}