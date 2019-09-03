using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacantLot.VLUtil;
using System;

namespace VacantLot.VLVTuber
{
    public class HandPoseManager : MonoBehaviour
    {
        public float changeDuration = 0.3f;
        public HandPoseDefinition LeftDefinition;
        public HandPoseDefinition RightDefinition;

        [Range(0, 1)]
        public float LeftClench;
        [Range(0, 1)]
        public float RightClench;
        [SerializeField]
        AnimationCurve ClenchCurve;

        HandPoseApplyer Left;
        HandPoseApplyer Right;

        void Awake()
        {
            Left = new HandPoseApplyer(this, LR.L);
            Right = new HandPoseApplyer(this, LR.R);
        }


        // Use this for initialization
        void Start()
        {
            Clear(LR.L);
            Clear(LR.R);
        }

        // Update is called once per frame
        void Update()
        {
            Left.Upadate(ClenchCurve.Evaluate(LeftClench));
            Right.Upadate(ClenchCurve.Evaluate(RightClench));
        }

        public void Apply(LR lr, int index) => GetApplyer(lr).Apply(index, changeDuration);
        public void ApplyLeft(int index) => Apply(LR.L, index);
        public void ApplyRight(int index) => Apply(LR.R, index);

        public void Clear(LR lr) => GetApplyer(lr).Neutralize(changeDuration);
        public void ClearLeft() => Clear(LR.L);
        public void ClearRight() => Clear(LR.R);

        [Serializable]
        public class HandPoseDefinition
        {
            public Transform RootBone;
            public TransformSnap NeutralPose;
            public TransformSnap ClenchPose;
            public TransformSnap[] Poses;

            public TransformSnap GetPose(int index)
            {
                if (index < 0 || index >= Poses.Length) return NeutralPose;

                return Poses[index] ?? NeutralPose;
            }
        }

        class HandPoseApplyer
        {
            public HandPoseApplyer(HandPoseManager owner, LR lr)
            {
                Owner = owner;
                LR = lr;
            }

            HandPoseManager Owner { get; }
            LR LR { get; }

            HandPoseDefinition Definition => Owner.GetDefinition(LR);
            TransformSnap Current;
            public TransitionPlayer<TransformSnap> Transition { get; private set; }

            public void Apply(int index, float duration) => Apply(Definition.GetPose(index), duration);

            public void Neutralize(float duration) => Apply(Definition.NeutralPose, duration);

            void Apply(TransformSnap snap, float duration)
            {
                if (!Definition.RootBone) return;
                if (snap == Current) return;

                Current = snap;

                Transition = new TransitionPlayer<TransformSnap>(
                    new Transition<TransformSnap>(TransformSnap.Lerp).AddKey(snap, duration),
                    TransformSnap.Snap(Definition.RootBone)
                    );
            }

            public void Upadate(float clench)
            {
                if (!Definition.RootBone) return;

                // 手のポーズをアニメーションしつつ握りと合成する
                TransformSnap.Lerp(
                    Transition?.Next(Time.deltaTime),
                    Definition.ClenchPose,
                    clench)
                    .Apply(Definition.RootBone);
            }
        }

        public enum LR
        {
            L,
            R,
        }

        public HandPoseDefinition GetDefinition(LR lr)
        {
            switch (lr)
            {
                case LR.L:
                    return LeftDefinition;
                case LR.R:
                    return RightDefinition;
            }
            throw new System.Exception("bug");
        }

        HandPoseApplyer GetApplyer(LR lr)
        {
            switch (lr)
            {
                case LR.L:
                    return Left;
                case LR.R:
                    return Right;
            }
            throw new System.Exception("bug");
        }
    }
}
