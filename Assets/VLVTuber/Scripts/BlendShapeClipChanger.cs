using System.Collections.Generic;
using UnityEngine;
using VRM;
using System.Linq;
using VacantLot.VLUtil;

namespace VacantLot.VLVTuber
{
    /// <summary>
    /// BlendShapeClip をアニメーションさせつつ適用する機能
    /// </summary>
    public partial class BlendShapeClipChanger : MonoBehaviour
    {
        public VRMBlendShapeProxy VRM;
        public float AutoResetSpeed = 0.5f;

        public bool IsActive => enabled && gameObject.activeInHierarchy && VRM;

        void Update()
        {
            var valuesToSet = new Dictionary<BlendShapeKey, float>();
            var keysFinished = new List<BlendShapeKey>();
            foreach (var t in transitions)
            {
                valuesToSet.Add(t.Key, t.Value.Next(Time.deltaTime));
                if (t.Value.IsFinished)
                {
                    keysFinished.Add(t.Key);
                }
            }

            if (!IsActive) return;
            VRM.SetValues(valuesToSet);

            // アニメーションが終わったものはリセットする
            // （リセットさせたくないものは Transition.Hold() でアニメーションを維持すること）
            foreach (var k in keysFinished)
            {
                transitions.Remove(k);
                if (VRM.GetValue(k) != 0)
                {
                    TurnDown(k, VRM.GetValue(k) * AutoResetSpeed);
                }
            }
        }

        public void AppendIfNot(BlendShapeKey clipKey, Transition<float> transition)
        {
            if (!IsActive) return;
            if (!IsAnimating(clipKey)) transitions.Add(clipKey, new TransitionPlayer<float>(transition, VRM.GetValue(clipKey)));
        }

        public void AppendIfNot(string clipName, Transition<float> transition) => AppendIfNot(BlendShapeKeyCache.Get(clipName), transition);

        public void AppendIfNot(BlendShapePreset clipPreset, Transition<float> transition) => AppendIfNot(BlendShapeKeyCache.Get(clipPreset), transition);

        public void Append(BlendShapeKey clipKey, Transition<float> transition)
        {
            transitions.Remove(clipKey);
            transitions.Add(clipKey, new TransitionPlayer<float>(transition, VRM.GetValue(clipKey)));
        }

        public void Append(string clipName, Transition<float> transition) => Append(BlendShapeKeyCache.Get(clipName), transition);

        public void Append(BlendShapePreset clipPreset, Transition<float> transition) => Append(BlendShapeKeyCache.Get(clipPreset), transition);

        public void TurnDown(BlendShapeKey clipKey, float duration) => Append(clipKey, CreateTransition().AddKey(0, duration));

        public void TurnDown(string clipName, float duration) => TurnDown(BlendShapeKeyCache.Get(clipName), duration);

        public void TurnDown(BlendShapePreset clipPreset, float duration) => TurnDown(BlendShapeKeyCache.Get(clipPreset), duration);

        public void ResetAll(float duration, bool appendNeutral = true)
        {
            transitions.Clear();

            if (!IsActive) return;
            VRM.BlendShapeAvatar.Clips.Select(_ => _.name).Where(_ => VRM.GetValue(_) != 0f && NeutralKey.Name != _)
                .ToList().ForEach(_ =>
                {
                    TurnDown(_, duration);
                });

            if (appendNeutral)
            {
                Append(NeutralKey, CreateTransition().AddKey(1.0f, duration).Hold());
            }
        }

        public bool IsAnimating(BlendShapeKey clipKey) => transitions.ContainsKey(clipKey);

        public bool IsAnimating(string clipName) => IsAnimating(BlendShapeKeyCache.Get(clipName));

        public bool IsAnimating(BlendShapePreset clipPreset) => IsAnimating(BlendShapeKeyCache.Get(clipPreset));

        public static Transition<float> CreateTransition() => new Transition<float>(Mathf.Lerp);
    }

    public partial class BlendShapeClipChanger : MonoBehaviour
    {
        Dictionary<BlendShapeKey, TransitionPlayer<float>> transitions = new Dictionary<BlendShapeKey, TransitionPlayer<float>>();
        static readonly BlendShapeKey NeutralKey = new BlendShapeKey(BlendShapePreset.Neutral);

    }

    public static class BlendShapeKeyCache
    {
        static Dictionary<string, BlendShapeKey> cache4string = new Dictionary<string, BlendShapeKey>();
        static Dictionary<BlendShapePreset, BlendShapeKey> cache4preset = new Dictionary<BlendShapePreset, BlendShapeKey>();

        static public BlendShapeKey Get(string name)
        {
            if (!cache4string.ContainsKey(name))
            {
                cache4string.Add(name, new BlendShapeKey(name));
            }
            return cache4string[name];
        }

        static public BlendShapeKey Get(BlendShapePreset preset)
        {
            if (!cache4preset.ContainsKey(preset))
            {
                cache4preset.Add(preset, new BlendShapeKey(preset));
            }
            return cache4preset[preset];
        }
    }
}