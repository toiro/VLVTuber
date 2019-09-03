using System;
using System.Collections;
using UnityEngine;
using VRM;
using VacantLot.VLUtil;


namespace VacantLot.VLVTuber
{
    /// <summary>
    /// VRM の BlendShapeClip による表情制御
    /// 表情パターンはすべて BlendShapeClip で定義されていて、それらを組み合わせることはしない前提
    /// </summary>
    public partial class FacialController : MonoBehaviour
    {
        public BlendShapeClipChanger changer;
        public float ChangeDuration = 0.2f;
        public bool IsBlinkActive = true;
        public bool UseBlinkLR = true;
        public BlinkParameterSet blinkParameters = new BlinkParameterSet();

        //public FacialExpressionAssign[] assigns; 
        public FacialExpression[] expressions;

        public bool IsActive => enabled && gameObject.activeInHierarchy;

        void Start()
        {
            blinker = new Blinker(this);
        }

        public void Invoke(string clipName) => Invoke(BlendShapeKeyCache.Get(clipName));

        public void Invoke(BlendShapePreset preset) => Invoke(new BlendShapeKey(preset));

        public void Invoke(BlendShapeKey Key)
        {
            if (!IsActive) return;
            foreach (var e in expressions)
            {
                if (e.ActualKey.Equals(Key))
                {
                    if (!changer.IsAnimating(e.ActualKey))
                    {
                        IsBlinkActive = e.CanBlink;
                        blinker.TightnessCollection = e.BlinkTightnessCollection;
                        blinker.TurnDown();

                        changer.Append(e.ActualKey, e.CreateTransition(ChangeDuration));
                    }
                }
                else
                {
                    // 指定以外の表情を解除する
                    changer.TurnDown(e.ActualKey, ChangeDuration);
                }
            }
        }

        public void Reset()
        {
            if (!IsActive) return;
            foreach (var e in expressions)
            {
                changer.TurnDown(e.ActualKey, ChangeDuration);
            }

            blinker.TightnessCollection = 1.0f;
            blinker.TurnDown();
        }

        public void InvokeBlink() => blinker.Invoke();

        [Serializable]
        public class FacialExpression
        {
            [Tooltip("適用するBlendShapeClip")][SerializeField]
            BlendShapeKey Key;

            [Tooltip("BlendShapeClip の適用度合い"), Range(0, 1.0f)]
            public float Weight = 1.0f;
            [Tooltip("まばたきさせるかどうか")]
            public bool CanBlink = true;
            [Tooltip("まばたきさせる場合のまばたきの強さの補正値"), Range(0.5f, 1.5f)]
            public float BlinkTightnessCollection = 1.0f;

            public BlendShapeKey ActualKey => new BlendShapeKey(Key.Name, Key.Preset);
            public Transition<float> CreateTransition(float duration) => BlendShapeClipChanger.CreateTransition().AddKey(Weight, duration).Hold();
        }

        [Serializable]
        public class BlinkParameterSet
        {
            [Range(0, 1.0f)]
            public float ratioHalf = 0.3f;
            [Range(0, 1.0f)]
            public float ratioClose = 0.9f;
            public float closeDuration = 0.1f;
            public float openDuration = 0.2f;
            public float interval = 1.5f;
            [Range(0, 1.0f)]
            public float randomThreshold = 0.7f;
        }
    }
    public partial class FacialController : MonoBehaviour
    {
        private Blinker blinker { get; set; }

        class Blinker
        {
            static readonly BlendShapeKey BlinkL = BlendShapeKeyCache.Get(BlendShapePreset.Blink_L);
            static readonly BlendShapeKey BlinkR = BlendShapeKeyCache.Get(BlendShapePreset.Blink_R);
            static readonly BlendShapeKey Blink = BlendShapeKeyCache.Get(BlendShapePreset.Blink);

            FacialController owner;
            BlinkParameterSet param => owner.blinkParameters;
            BlendShapeClipChanger changer => owner.changer;

            public float TightnessCollection { set; get; }

            private bool _UseLR;
            public bool UseLR {
                set
                {
                    if (_UseLR == value) return;
                    if (changer == null) return;
                    TurnDown();
                    _UseLR = value;
                }
                get { return _UseLR; }
            }

            public Blinker(FacialController owner)
            {
                this.owner = owner;
                owner.StartCoroutine(BlinkSignaler());
                TightnessCollection = 1.0f;
            }

            public void Invoke()
            {
                if(UseLR)
                {
                    changer.AppendIfNot(BlinkL, CreateBlinkTransition());
                    changer.AppendIfNot(BlinkR, CreateBlinkTransition());
                }
                else
                {
                    changer.AppendIfNot(Blink, CreateBlinkTransition());
                }
            }

            public void TurnDown()
            {
                if(UseLR)
                {
                    changer.TurnDown(Blinker.BlinkL, owner.ChangeDuration);
                    changer.TurnDown(Blinker.BlinkR, owner.ChangeDuration);
                }
                else
                {
                    changer.TurnDown(Blinker.Blink, owner.ChangeDuration);
                }
            }

            // ランダム判定用関数
            IEnumerator BlinkSignaler()
            {
                // 無限ループ開始
                while (true)
                {
                    if (owner.IsActive && owner.IsBlinkActive && UnityEngine.Random.value > param.randomThreshold)
                    {
                        Invoke();
                    }

                    // 次の判定までインターバルを置く
                    yield return new WaitForSeconds(param.interval);
                }
            }

            Transition<float> CreateBlinkTransition()
            {
                var partCloseDuration = param.closeDuration / 4;
                var partOpenDuration = param.openDuration / 4;
                return BlendShapeClipChanger.CreateTransition()
                    .AddKey(param.ratioHalf * TightnessCollection, partCloseDuration)
                    .AddKey(param.ratioClose * TightnessCollection, partCloseDuration)
                    .AddKey(param.ratioHalf * TightnessCollection, partOpenDuration)
                    .AddKey(0, partOpenDuration);
            }
        }
    }
}
