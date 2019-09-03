using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacantLot.VLUtil;

namespace VacantLot.VLVTuber
{
    public class TrackingTargetProxy
    {
        public bool IsValid => OriginalTarget && ActualTarget;

        public Transform OriginalTarget { get; }
        public Transform ActualTarget { get; }
        private Transform Stabilizer { get; }
        private Transform Offset { get; }

        public TrackingTargetProxy(Transform trackingTarget)
        {
            OriginalTarget = trackingTarget;
            var prefix = "_" + OriginalTarget.name + "_TrackingTargetProxy";
            Stabilizer = new GameObject(prefix + "_Stabilizer").transform;
            Stabilizer.gameObject.AddComponent<TrackerStabilizer>();
            Offset = new GameObject(prefix + "_Offset").transform;
            ActualTarget = Offset;

            Stabilizer.parent = OriginalTarget;
            Offset.parent = Stabilizer;
        }

        public TransformValue OffsetValue { set { value.Apply(Offset); } }
        public int StabilizerSamplingSize { set { Stabilizer.GetComponent<TrackerStabilizer>().SamplingSize = value; } }

        public void DestroyProxy()
        {
            if(Stabilizer)
            {
                Object.Destroy(Stabilizer.gameObject);
            }
            if(Offset)
            {
                Object.Destroy(Offset.gameObject);
            }
        }
    }
}
