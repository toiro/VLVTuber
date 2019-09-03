using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace VacantLot.VLUtil
{
    public static class Extensions
    {
        /// <summary>
        /// Jason 形式に変換することでオブジェクトをクローンする。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">クローン元にするオブジェクト</param>
        /// <returns>クローンしたオブジェクト</returns>
        public static T JsonClone<T>(this T src)
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(src));
        }

        #region Decendants
        /// <summary>
        /// Transform の子孫要素の列挙を返す。自身は含まない。
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>列挙</returns>
        public static IEnumerable<Transform> Decendants(this Transform transform)
        {
            return new DecendantsEnumerable(transform);
        }

        private class DecendantsEnumerable : IEnumerable<Transform>
        {
            private Transform root;
            public DecendantsEnumerable(Transform root)
            {
                this.root = root;
            }

            public IEnumerator<Transform> GetEnumerator() => new DecendantsScanner(root);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class DecendantsScanner : IEnumerator<Transform>
            {
                public Transform Current { get; private set; }
                private Stack<Transform> stack = new Stack<Transform>();

                public DecendantsScanner(Transform root)
                {
                    stack.Push(root);
                    // root を含まない
                    MoveNext();
                }

                public bool MoveNext()
                {
                    if (stack.Count == 0) return false;

                    Current = stack.Pop();
                    foreach (Transform child in Current)
                    {
                        stack.Push(child);
                    }

                    return true;
                }

                void IDisposable.Dispose()
                {
                    stack.Clear();
                    stack = null;
                }

                object IEnumerator.Current => Current;

                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }
            }
        }
        #endregion
    }
}
