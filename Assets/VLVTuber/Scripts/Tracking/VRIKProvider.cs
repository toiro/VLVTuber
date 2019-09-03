using System;
using System.Linq;
using UnityEngine;
using RootMotion.FinalIK;
using VacantLot.VLUtil;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VacantLot.VLVTuber
{
    /// <summary>
    /// VRIK を動的に設定する機能を提供する。
    /// </summary>
    public partial class VRIKProvider : MonoBehaviour
    {
        public GameObject Avatar;
        public GameObject VRRig;
        public TrackingStyle Style;
        public TrackingTargetSet TrackingTargets;
        public AdjustParameterSet AdjustParameters = new AdjustParameterSet();
        [SerializeField]
        internal IKSolverVR solverParameter = new IKSolverVR();
        [Range(1, 60)]
        public int TrackingStabilizerSamplingSize = 5;

        public VRIK.References References => provided?.references;
        public IKBoneRotations AvatarDefaultRotations { private set; get; }

        public bool IsActive => enabled && gameObject.activeInHierarchy;

        // Use this for initialization
        void Start()
        {
            CallSetUp();
        }

        /// <summary>
        /// VRIK を設定する。
        /// </summary>
        public void CallSetUp()
        {
            if (!IsActive) return;
            StartCoroutine(SetUp());
        }

        public bool IsToSetUp { get; private set; } = false;
        public IEnumerator SetUp()
        {
            if (!IsToSetUp)
            {
                IsToSetUp = true;
                yield return new WaitForFixedUpdate();
                try
                {
                    _SetUp();
                }
                finally
                {
                    IsToSetUp = false;
                }
            }
        }

        private void _SetUp()
        {
            if (!enabled || !Avatar) return;

            if (!provided || provided?.gameObject != Avatar)
            {
                // whole setup
                SetUpScale();
                SetUpVRIK();
                ApplyTrackingStyle(provided.solver);
                AdjustParameters.AjdustSolverScale(provided.solver, solverParameter);
                SetUpTargetProxies();
            }
            else
            {
                // update exists components
                SetUpScale();
                ApplyTrackingStyle(provided.solver);
                AdjustParameters.AjdustSolverScale(provided.solver, solverParameter);
                SetUpTargetProxies();
            }
        }

        public void Dispose()
        {
            DestroyProvided();
        }

        void OnDestroy()
        {
            DestroyProvided();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!EditorApplication.isPlaying || !IsActive) return;

            EditorApplication.delayCall += _SetUp;
        }
#endif
    }

    partial class VRIKProvider : MonoBehaviour
    {
        VRIK provided;
        InstnaceIDDictionary<Transform, TrackingTargetProxy> proxies = new InstnaceIDDictionary<Transform, TrackingTargetProxy>();

        void SetUpScale()
        {
            vrRigOriginalScale.RetainIfNot();
            avatarOriginalScale.RetainIfNot();

            // 体型補正
            var scale = vrRigOriginalScale.Value * AdjustParameters.VRArmScale;
            scale = new Vector3(
                scale.x,
                scale.y * AdjustParameters.VRHeightScale,
                scale.z
                );
            VRRig.transform.localScale = scale;

            // VR内縮尺
            Avatar.transform.localScale = avatarOriginalScale.Value * AdjustParameters.AvatarScale;
        }

        void SetUpTargetProxies()
        {
            foreach (TrackingTarget t in Enum.GetValues(typeof(TrackingTarget)))
            {
                var target = GetTarget(provided.solver, t);

                if (!target) continue;

                if (proxies.ContainsKey(target) && proxies[target].IsValid)
                {
                    GetOffset(t).UpdateOffset(proxies[target]);
                }
                else
                {
                    proxies.Add(target, GetOffset(t).CreateProxy(GetTarget(provided.solver, t)));
                }

                SetTarget(provided.solver, t, proxies[target].ActualTarget);
                proxies[target].StabilizerSamplingSize = TrackingStabilizerSamplingSize;
            }
        }

        private void SetUpVRIK()
        {
            DestroyProvided();

            // 既存の VRIK を切る（フリーズなどで破棄に失敗することがある……）
            Avatar.GetComponents<VRIK>().ToList().ForEach(_ => _.enabled = false);

            provided = Avatar.AddComponent<VRIK>();
            provided.AutoDetectReferences();

            AvatarDefaultRotations = new IKBoneRotations(References);

            provided.solver = solverParameter.JsonClone();
            // 膝方向の自動判定のために膝を曲げる
            leftKneeLocalRotation.RetainIfNot();
            rightKneeLocalRotation.RetainIfNot();
            provided.references.leftCalf.localRotation = Quaternion.Euler(AdjustParameters.KneeBending) * leftKneeLocalRotation.Value;
            provided.references.rightCalf.localRotation = Quaternion.Euler(AdjustParameters.KneeBending) * rightKneeLocalRotation.Value;

        }

        private void ApplyTrackingStyle(IKSolverVR solver)
        {
            TrackingTargets.ApplyAll(provided.solver, Style);

            solver.spine.pelvisPositionWeight = Style.TrackPelvis ? solverParameter.spine.pelvisPositionWeight : 0;
            solver.spine.pelvisRotationWeight = Style.TrackPelvis ? solverParameter.spine.pelvisRotationWeight : 0;

            solver.rightLeg.positionWeight = Style.TrackFeet ? solverParameter.rightLeg.positionWeight : 0;
            solver.rightLeg.rotationWeight = Style.TrackFeet ? solverParameter.rightLeg.rotationWeight : 0;
            solver.leftLeg.positionWeight = Style.TrackFeet ? solverParameter.leftLeg.positionWeight : 0;
            solver.leftLeg.rotationWeight = Style.TrackFeet ? solverParameter.leftLeg.rotationWeight : 0;
        }

        void DestroyProvided()
        {
            if (!provided) return;

            Destroy(provided);

            foreach (var p in proxies)
            {
                p.Value.DestroyProxy();
            }
            proxies.Clear();

            this.RevertRetainedValues();
            this.DisposeRetainedValueAccessors();
        }

        [Serializable]
        public struct TrackingStyle
        {
            public bool UseHMD;
            public bool TrackPelvis;
            public bool TrackFeet;
        }

        [Serializable]
        public struct TrackingTargetSet
        {
            public Transform HMD;
            public Transform HeadTracker;
            public Transform PelvisTracker;
            public Transform LeftHandTracker;
            public Transform RightHandTracker;
            public Transform LeftFootTracker;
            public Transform RightFootTracker;

            public void ApplyAll(IKSolverVR solver, TrackingStyle style)
            {
                solver.spine.headTarget = style.UseHMD ? HMD : HeadTracker;
                solver.leftArm.target = LeftHandTracker;
                solver.rightArm.target = RightHandTracker;
                solver.spine.pelvisTarget = style.TrackPelvis ? PelvisTracker : null;
                solver.leftLeg.target = style.TrackFeet ? LeftFootTracker : null;
                solver.rightLeg.target = style.TrackFeet ? RightFootTracker : null;
            }

            public Transform Get(TrackingTarget target, bool UseHMD = false)
            {
                switch (target)
                {
                    case TrackingTarget.Head:
                        return UseHMD ? HMD : HeadTracker;
                    case TrackingTarget.Pelvis:
                        return PelvisTracker;
                    case TrackingTarget.LeftHand:
                        return LeftHandTracker;
                    case TrackingTarget.RightHand:
                        return RightHandTracker;
                    case TrackingTarget.LeftLeg:
                        return LeftFootTracker;
                    case TrackingTarget.RightLeg:
                        return RightFootTracker;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [Serializable]
        public class AdjustParameterSet
        {
            public float VRArmScale = 1;
            public float VRHeightScale = 1;
            public float AvatarScale = 1;
            public TrackingOffsetSet TrackingOffset;
            public Vector3 KneeBending = new Vector3(5, 0, 0);


            public void AjdustSolverScale(IKSolverVR solver, IKSolverVR solverParameter)
            {
                // 参考: https://qiita.com/chiepomme/items/aef42df2d46aa0f79fbe
                solver.spine.minHeadHeight = solverParameter.spine.minHeadHeight * AvatarScale;

                solver.locomotion.footDistance = solverParameter.locomotion.footDistance * AvatarScale;
                solver.locomotion.stepThreshold = solverParameter.locomotion.stepThreshold * AvatarScale;
                solver.locomotion.maxVelocity = solverParameter.locomotion.maxVelocity * AvatarScale;

                for (var i = 0; i < solver.locomotion.stepHeight.keys.Length; i++)
                {
                    var newKey = solverParameter.locomotion.stepHeight.keys[i];
                    newKey.value *= AvatarScale;
                    solver.locomotion.stepHeight.MoveKey(i, newKey);
                }

                for (var i = 0; i < solver.locomotion.heelHeight.keys.Length; i++)
                {
                    var newKey = solverParameter.locomotion.heelHeight.keys[i];
                    newKey.value *= AvatarScale;
                    solver.locomotion.heelHeight.MoveKey(i, newKey);
                }
            }

            [Serializable]
            public struct TrackingOffsetSet
            {
                public TrackingOffset HMD;
                public TrackingOffset Head;
                public TrackingOffset Pelvis;
                public TrackingOffset LeftHand;
                public TrackingOffset RightHand;
                public TrackingOffset LeftFoot;
                public TrackingOffset RightFoot;

                [Serializable]
                public class TrackingOffset
                {
                    public Vector3 positionOffset;
                    public Vector3 rotationOffset;

                    public TrackingOffset(Vector3 positionOffset, Vector3 rotationOffset)
                    {
                        this.positionOffset = positionOffset;
                        this.rotationOffset = rotationOffset;
                    }

                    public TrackingTargetProxy CreateProxy(Transform target)
                    {
                        var proxy = new TrackingTargetProxy(target);
                        UpdateOffset(proxy);
                        return proxy;
                    }

                    public void UpdateOffset(TrackingTargetProxy proxy)
                    {
                        var rotation = Quaternion.Euler(rotationOffset);
                        // 位置補正は回転と無関係にかける
                        // TODO モデルの回転分は無視する必要あり？
                        proxy.OffsetValue = new TransformValue(rotation * positionOffset, rotation, Vector3.one);
                    }
                }
            }
        }

        [Serializable]
        public struct IKBoneRotations
        {
            public Quaternion root;
            public Quaternion pelvis;
            public Quaternion head;
            public Quaternion leftForearm;
            public Quaternion leftHand;
            public Quaternion rightForearm;
            public Quaternion rightHand;
            public Quaternion leftCalf;
            public Quaternion leftFoot;
            public Quaternion rightCalf;
            public Quaternion rightFoot;

            public IKBoneRotations(VRIK.References references)
            {
                root = references.root.rotation;
                pelvis = references.pelvis.rotation;
                head = references.head.rotation;
                leftForearm = references.leftForearm.rotation;
                leftHand = references.leftHand.rotation;
                rightForearm = references.rightForearm.rotation;
                rightHand = references.rightHand.rotation;
                leftCalf = references.leftCalf.rotation;
                leftFoot = references.leftFoot.rotation;
                rightCalf = references.rightCalf.rotation;
                rightFoot = references.rightFoot.rotation;
            }
        }

        #region TrackingTarget
        public enum TrackingTarget
        {
            Head,
            Pelvis,
            LeftHand,
            RightHand,
            LeftLeg,
            RightLeg,
            //LeftArmBend,
            //RightArmBend,
            //LeftLegBend,
            //RightLegBend,
        }

        private static Transform GetTarget(IKSolverVR solver, TrackingTarget t)
        {
            switch (t)
            {
                case TrackingTarget.Head:
                    return solver.spine.headTarget;
                case TrackingTarget.Pelvis:
                    return solver.spine.pelvisTarget;
                case TrackingTarget.LeftHand:
                    return solver.leftArm.target;
                case TrackingTarget.RightHand:
                    return solver.rightArm.target;
                case TrackingTarget.LeftLeg:
                    return solver.leftLeg.target;
                case TrackingTarget.RightLeg:
                    return solver.rightLeg.target;
                default:
                    // TODO
                    return null;
            }
        }

        private static void SetTarget(IKSolverVR solver, TrackingTarget t, Transform target)
        {
            switch (t)
            {
                case TrackingTarget.Head:
                    solver.spine.headTarget = target;
                    break;
                case TrackingTarget.Pelvis:
                    solver.spine.pelvisTarget = target;
                    break;
                case TrackingTarget.LeftHand:
                    solver.leftArm.target = target;
                    break;
                case TrackingTarget.RightHand:
                    solver.rightArm.target = target;
                    break;
                case TrackingTarget.LeftLeg:
                    solver.leftLeg.target = target;
                    break;
                case TrackingTarget.RightLeg:
                    solver.rightLeg.target = target;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        AdjustParameterSet.TrackingOffsetSet.TrackingOffset GetOffset(TrackingTarget t)
        {
            switch (t)
            {
                case TrackingTarget.Head:
                    return Style.UseHMD ? AdjustParameters.TrackingOffset.HMD : AdjustParameters.TrackingOffset.Head;
                case TrackingTarget.Pelvis:
                    return AdjustParameters.TrackingOffset.Pelvis;
                case TrackingTarget.LeftHand:
                    return AdjustParameters.TrackingOffset.LeftHand;
                case TrackingTarget.RightHand:
                    return AdjustParameters.TrackingOffset.RightHand;
                case TrackingTarget.LeftLeg:
                    return AdjustParameters.TrackingOffset.LeftFoot;
                case TrackingTarget.RightLeg:
                    return AdjustParameters.TrackingOffset.RightFoot;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region RetainedValue
        RetainedValueStore.IAccessor<Vector3> _vrRigOriginalScale;
        RetainedValueStore.IAccessor<Vector3> vrRigOriginalScale
        {
            get
            {
                if (!RetainedValueStore.IsValid(_vrRigOriginalScale))
                {
                    _vrRigOriginalScale = this.RegistValueRetainer(
                        () => VRRig.transform.localScale,
                        _ => VRRig.transform.localScale = _);
                }
                return _vrRigOriginalScale;
            }
        }
        RetainedValueStore.IAccessor<Vector3> _avatarOriginalScale;
        RetainedValueStore.IAccessor<Vector3> avatarOriginalScale
        {
            get
            {
                if (!RetainedValueStore.IsValid(_avatarOriginalScale))
                {
                    _avatarOriginalScale = this.RegistValueRetainer(
                        () => Avatar.transform.localScale,
                       _ => Avatar.transform.localScale = _);
                }
                return _avatarOriginalScale;
            }
        }
        RetainedValueStore.IAccessor<Quaternion> _leftKneeLocalRotation;
        RetainedValueStore.IAccessor<Quaternion> leftKneeLocalRotation
        {
            get
            {
                if (!RetainedValueStore.IsValid(_leftKneeLocalRotation))
                {
                    _leftKneeLocalRotation = this.RegistValueRetainer(
                        () => provided.references.leftCalf.localRotation,
                        _ => provided.references.leftCalf.localRotation = _);
                }
                return _leftKneeLocalRotation;
            }
        }
        RetainedValueStore.IAccessor<Quaternion> _rightKneeLocalRotation;
        RetainedValueStore.IAccessor<Quaternion> rightKneeLocalRotation
        {
            get
            {
                if (!RetainedValueStore.IsValid(_rightKneeLocalRotation))
                {
                    _rightKneeLocalRotation = this.RegistValueRetainer(
                        () => provided.references.rightCalf.localRotation,
                        _ => provided.references.rightCalf.localRotation = _);
                }
                return _rightKneeLocalRotation;
            }
        }
        #endregion

    }

    partial class VRIKProvider : MonoBehaviour
    {
        Vector3 WorldForward => VRRig.transform.rotation * Vector3.forward;
        Vector3 WorldUpward => VRRig.transform.rotation * Vector3.up;
        RotationOrientation WorldReferenceDirection => new RotationOrientation(WorldForward, WorldUpward);

        Vector3 AvatarForward => AvatarDefaultRotations.root * Vector3.forward;
        Vector3 AvatarUpward => AvatarDefaultRotations.root * Vector3.up;
        RotationOrientation AvatarReferenceDirection => new RotationOrientation(AvatarForward, AvatarUpward);

        static Quaternion CalcOffsetRotation(Quaternion trackerRotation, RotationOrientation trackerDirectionReference, Quaternion avatarPartDefaultRotation, RotationOrientation avataReferencerDirection)
        {
            // トラッカーの回転分を打ち消し、アバターの回転分を付け足す
            return Quaternion.Inverse(trackerDirectionReference.ConvertToGlobal(trackerRotation)) * avataReferencerDirection.ConvertToLocal(avatarPartDefaultRotation);
        }

        Quaternion CalcOffsetRotation(Transform tracker, Quaternion avatarPartDefaultRotation) =>
            CalcOffsetRotation(tracker.rotation, WorldReferenceDirection, avatarPartDefaultRotation, AvatarReferenceDirection);

        public Quaternion CalcOffsetRotation(TrackingTarget target, bool useHMD)
        {
            switch (target)
            {
                case TrackingTarget.Head:
                    if (useHMD)
                    {
                        return CalcOffsetRotation(TrackingTargets.HMD, AvatarDefaultRotations.head);
                    }
                    else
                    {
                        return CalcOffsetRotation(TrackingTargets.HeadTracker, AvatarDefaultRotations.head);
                    }
                case TrackingTarget.Pelvis:
                    return CalcOffsetRotation(TrackingTargets.PelvisTracker, AvatarDefaultRotations.pelvis);
                case TrackingTarget.LeftHand:
                    return CalcOffsetRotation(TrackingTargets.LeftHandTracker, AvatarDefaultRotations.leftHand);
                case TrackingTarget.RightHand:
                    return CalcOffsetRotation(TrackingTargets.RightHandTracker, AvatarDefaultRotations.rightHand);
                case TrackingTarget.LeftLeg:
                    return CalcOffsetRotation(TrackingTargets.LeftFootTracker, AvatarDefaultRotations.leftFoot);
                case TrackingTarget.RightLeg:
                    return CalcOffsetRotation(TrackingTargets.RightFootTracker, AvatarDefaultRotations.rightFoot);

                default:
                    throw new NotImplementedException();
            }
        }

        public void UpdateRotationOffset(TrackingTarget target, int waitSecond, bool useHMD = false)
        {
            StartCoroutine(UpdateRotaionOffsetCoroutine(target, waitSecond, useHMD));
        }

        IEnumerator UpdateRotaionOffsetCoroutine(TrackingTarget target, int waitSecond, bool useHMD = false)
        {
            if (waitSecond < 0) waitSecond = 0;
            for (int countDown = waitSecond; countDown > 0; countDown--)
            {
                Debug.LogFormat("UpdateRotationOffset Count: {0}", countDown);
                yield return new WaitForSeconds(1f);
            }
            Debug.LogFormat("UpdateRotationOffset finished");
            GetOffset(target).rotationOffset = CalcOffsetRotation(target, useHMD).eulerAngles;
            if (Application.isPlaying) CallSetUp();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VRIKProvider))]
    public class VRIKProviderEditor : Editor
    {
        private VRIKProvider script { get { return target as VRIKProvider; } }

        void OnEnable()
        {
            if (serializedObject == null) return;

            // Changing the script execution order
            if (!Application.isPlaying)
            {
                // stepHeight の初期値設定
                script.solverParameter.DefaultAnimationCurves();
            }
        }
    }
#endif

}