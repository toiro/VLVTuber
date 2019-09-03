using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VacantLot.VLUtil
{
    /// <summary>
    /// 回転の基準方向
    /// </summary>
    public class RotationOrientation
    {
        public Vector3 Forward { get; }
        public Vector3 Upward { get; }
        public Quaternion LookRotation { get; }

        public RotationOrientation(Vector3 Forward, Vector3 Upward)
        {
            this.Forward = Forward;
            this.Upward = Upward;
            this.LookRotation = Quaternion.LookRotation(Forward, Upward);
        }


        /// <summary>
        /// 座標系方向からの回転に変換する
        /// </summary>
        /// <param name="GlobalRotation"></param>
        /// <returns></returns>
        public Quaternion ConvertToGlobal(Quaternion localRotation) => localRotation * LookRotation;

        /// <summary>
        /// 基準方向からの回転に変換する
        /// </summary>
        /// <param name="globalRotation"></param>
        /// <returns></returns>
        public Quaternion ConvertToLocal(Quaternion globalRotation) => Quaternion.Inverse(LookRotation) * globalRotation;
    }
}
