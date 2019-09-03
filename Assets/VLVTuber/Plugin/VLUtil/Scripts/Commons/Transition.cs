using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VacantLot.VLUtil
{
    // TODO: Unity の Animation との変換をする？

    /// <summary>
    /// パラメータ遷移定義
    /// </summary>
    /// <typeparam name="T">遷移パラメータのデータタイプ</typeparam>
    public class Transition<T>
    {
        List<TransitionKey<T>> _Keys;

        public Func<T, T, float, T> DefaultInterpolate { get; private set; }
        public IEnumerable<TransitionKey<T>> Keys { get { return _Keys.AsEnumerable(); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultInterpolate">既定の補間関数</param>
        public Transition(Func<T, T, float, T> defaultInterpolate)
        {
            this.DefaultInterpolate = defaultInterpolate;
            _Keys = new List<TransitionKey<T>>();
        }

        /// <summary>
        /// キーを追加する
        /// </summary>
        /// <param name="nextValue">キーの値</param>
        /// <param name="duration">キーの値になるまでの時間（前のキーを0とする）</param>
        /// <param name="interpolate">補間関数</param>
        /// <returns></returns>
        public Transition<T> AddKey(T nextValue, float duration, Func<T, T, float, T> interpolate = null)
        {
            _Keys.Add(new TransitionKey<T>(nextValue, duration, interpolate));
            return this;
        }

        /// <summary>
        /// 最終状態を保持するキーを追加する
        /// </summary>
        /// <returns></returns>
        public Transition<T> Hold()
        {
            AddKey(_Keys.Last().EndValue, float.MaxValue);
            return this;
        }
    }
    /// <summary>
    /// パラメータ遷移のキー要素
    /// </summary>
    /// <typeparam name="T">遷移パラメータのデータタイプ</typeparam>
    public class TransitionKey<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endValue">最終状態の値</param>
        /// <param name="duration">遷移時間</param>
        /// <param name="interpolate">補間関数</param>
        public TransitionKey(T endValue, float duration, Func<T, T, float, T> interpolate = null)
        {
            this.EndValue = endValue;
            this.Duration = duration;
            this.Interpolate = interpolate;
        }

        public T EndValue { get; private set; }
        public float Duration { get; private set; }
        public Func<T, T, float, T> Interpolate { get; private set; }
    }

    /// <summary>
    /// パラメータ遷移を実行する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransitionPlayer<T>
    {
        Queue<TransitionKey<T>> Future;
        Queue<TransitionKey<T>> Past;

        TransitionKey<T> PreviousKey;
        TransitionKey<T> CurrentKey;
        //float IntegralEstimate;
        float CurrentEstimate;
        Func<T, T, float, T> DefaultInterpolate;

        Func<T, T, float, T> Interpolate => CurrentKey?.Interpolate ?? DefaultInterpolate;
        public bool IsFinished { private set; get; }
        public bool Repeat { get; }

        public float Estimate => Past.Select(_ => _.Duration).Aggregate((total, elem) => total + elem) + CurrentEstimate;
        public float Duration => Past.Select(_ => _.Duration).Aggregate((total, elem) => total + elem) + CurrentKey.Duration + Future.Select(_ => _.Duration).Aggregate((total, elem) => total + elem);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transition">遷移定義</param>
        /// <param name="initialValue">初期値</param>
        /// <param name="repeat">遷移を繰り返すかどうか</param>
        public TransitionPlayer(Transition<T> transition, T initialValue, bool repeat = false)
        {
            this.DefaultInterpolate = transition.DefaultInterpolate;

            Future = new Queue<TransitionKey<T>>(transition.Keys);
            Past = new Queue<TransitionKey<T>>();
            PreviousKey = new TransitionKey<T>(initialValue, 0);
            CurrentKey = Future.Dequeue();
            CurrentEstimate = 0;
            IsFinished = false;
            Repeat = repeat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeDelta">経過時間</param>
        /// <returns>遷移中の値</returns>
        public T Next(float timeDelta)
        {
            Update(timeDelta);

            return Interpolate(PreviousKey.EndValue, CurrentKey.EndValue, CurrentEstimate / CurrentKey.Duration);
        }

        private void Update(float timeDelta)
        {
            // 中断 or 終了しているなら内部状態を更新しない
            if (IsFinished) return;

            CurrentEstimate += timeDelta;
            if (CurrentEstimate >= CurrentKey.Duration)
            {
                if (Future.Any())
                {
                    Past.Enqueue(CurrentKey);
                    PreviousKey = CurrentKey;
                    CurrentKey = Future.Dequeue();
                    CurrentEstimate -= CurrentKey.Duration;
                }
                else if (Repeat)
                {
                    Past.Enqueue(CurrentKey);
                    PreviousKey = CurrentKey;

                    var reincarnation = Future;
                    Future = Past;
                    Past = reincarnation;

                    CurrentKey = Future.Dequeue();
                    CurrentEstimate -= CurrentKey.Duration;
                }
                else
                {
                    IsFinished = true;
                    CurrentEstimate = CurrentKey.Duration;
                }
            }
        }

        public void Abort()
        {
            IsFinished = true;
        }
    }
}
