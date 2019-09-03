using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VacantLot.VLUtil
{
    public class TrackerStabilizer : MonoBehaviour
    {
        [Range(1,300)]
        public int SamplingSize = 5;
        public Timing UpdateTiming = Timing.Update;

        private LinkedList<TransformValue> Sample;
        private Func<IEnumerable<TransformValue>, TransformValue> EaseFunction { get; }


        public TrackerStabilizer()
        {
            EaseFunction = TransformValue.Average;
            Sample = new LinkedList<TransformValue>();
        }

        void Update()
        {
            if(UpdateTiming == Timing.Update)
            {
                UpdateSample();
                UpdateTransform();
            }
        }

        void LateUpdate()
        {
            if (UpdateTiming == Timing.LateUpdate)
            {
                UpdateSample();
                UpdateTransform();
            }
        }

        void UpdateSample()
        {
            Sample.AddLast(new TransformValue(transform.parent));
            while (Sample.Count > SamplingSize)
            {
                Sample.RemoveFirst();
            }
        }

        void UpdateTransform()
        {
            var easingValue = EaseFunction(Sample);

            transform.localPosition = easingValue.Position - transform.parent.localPosition;
            transform.localRotation = Quaternion.Inverse(transform.parent.localRotation) * easingValue.Rotation;
        }

        public enum Timing
        {
            Update,
            LateUpdate,
        }
    }
}
