using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VacantLot.VLUtil
{
    /// <summary>
    /// UnityEngine.Object にシリアライズ可能な値を一時的に保持する機能を提供する。
    /// 不要になった IAccesor は破棄することを推奨。
    /// マルチスレッドに対応しないため、メインスレッドでのみ使用すること。
    /// </summary>
    public static partial class RetainedValueStore
    {
        /// <summary>
        /// Unity オブジェクトに紐づけて値を保持する機能を取得する。
        /// </summary>
        /// <typeparam name="T">保持する値の型</typeparam>
        /// <param name="context">保持を紐づけるオブジェクト</param>
        /// <param name="getValueMethod">保持したい値を取得する方法</param>
        /// <param name="revertMethod">保持した値を戻す方法</param>
        /// <returns>機能を利用するためのアクセスオブジェクト</returns>
        public static IAccessor<T> RegistValueRetainer<T>(this UnityEngine.Object context, Func<T> getValueMethod, Action<T> revertMethod = null) => new Accessor<T>(context, getValueMethod, revertMethod);

        /// <summary>
        /// Unity オブジェクトに紐づいている保持された値をすべて戻す。
        /// </summary>
        /// <param name="context"></param>
        public static void RevertRetainedValues(this UnityEngine.Object context) => GetAccessorSet(context)?.ToList().ForEach(_ => _.Revert());

        /// <summary>
        /// Unity オブジェクトに紐づいている保持する機能をすべて破棄する。
        /// </summary>
        /// <param name="context"></param>
        public static void DisposeRetainedValueAccessors(this UnityEngine.Object context) => GetAccessorSet(context)?.ToList().ForEach(_ => _.Dispose());

        public static bool IsValid(IAccessor accessor) => accessor?.IsValid ?? false;

        public interface IAccessor<T> : IAccessor
        {
            /// <summary>
            /// 保持された値
            /// </summary>
            T Value { get; }
        }

        public interface IAccessor
        {
            /// <summary>
            /// 有効かどうか
            /// </summary>
            bool IsValid { get; }
            /// <summary>
            /// 値を保持しているかどうか
            /// </summary>
            bool HasRetained { get; }
            /// <summary>
            /// 値を保持していないなら、保持する
            /// </summary>
            void RetainIfNot();
            /// <summary>
            /// 保持している値を戻す
            /// </summary>
            void Revert();
            /// <summary>
            /// 保持している値を消去する
            /// </summary>
            void Clear();
            /// <summary>
            /// 保持する機能を破棄する
            /// </summary>
            void Dispose();
        }
    }

    public static partial class RetainedValueStore
    {
        static InstnaceIDDictionary<UnityEngine.Object, HashSet<IAccessor>> accessors = new InstnaceIDDictionary<UnityEngine.Object, HashSet<IAccessor>>();
        static Dictionary<IAccessor, object> valueStore = new Dictionary<IAccessor, object>();

        static HashSet<IAccessor> GetAccessorSet(UnityEngine.Object context) => accessors.ContainsKey(context) ? accessors[context] : null;

        static void AddAccessor(IAccessor accessor, UnityEngine.Object context)
        {
            CleanLazily();
            if(! accessors.ContainsKey(context))
            {
                accessors.Add(context, new HashSet<IAccessor>());
            }
            accessors[context].Add(accessor);
        }

        static void Clean() => accessors.Clean(removing => removing.ToList().ForEach(accessor => accessor.Dispose()));

        static void CleanLazily()
        {
            if (UnityEngine.Random.value < 0.01) Clean();
        }

        class Accessor<T> : IAccessor<T>
        {
            UnityEngine.Object context;
            Func<T> getValueMethod;
            Action<T> revertMethod;

            public Accessor(UnityEngine.Object context, Func<T> getValueMethod, Action<T> revertMethod)
            {
                this.context = context;
                this.getValueMethod = getValueMethod;
                this.revertMethod = revertMethod;
                AddAccessor(this, context);
            }

            public T Value => (T) (HasRetained? valueStore[this]: null);

            public bool IsValid => accessors.ContainsKey(context) && accessors[context].Contains(this);

            public bool HasRetained => valueStore.ContainsKey(this);

            public void Clear() => valueStore.Remove(this);

            public void Dispose()
            {
                Clear();
                accessors[context].Remove(this);
            }

            public void Revert()
            {
                if (HasRetained && revertMethod != null)
                {
                    revertMethod(Value);
                }
            }

            public void RetainForce() => Retain(true);

            public void RetainIfNot() => Retain(false);

            void Retain(bool force)
            {
                if (!IsValid) throw new Exception("Accessor is disposed.");
                if (!force && HasRetained) return;
                var value = getValueMethod();
                valueStore.Add(this, (value.GetType().IsPrimitive ? value : value.JsonClone()));
            }
        }
    }
}
