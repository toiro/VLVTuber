using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VacantLot.VLUtil
{
    /// <summary>
    /// 他シーンから型で参照可能な MonoBehaviour
    /// 1シーンにつき1インスタンスのみ参照可能
    /// </summary>
    public abstract class ReferableMonoBehaviour : MonoBehaviour
    {
        public static T Get<T>(Scene scene) where T : ReferableMonoBehaviour
        {
            if (!Accessors.ContainsKey(scene))
            {
                Debug.LogWarning(typeof(T).Name + " is not registered for scene: " + scene.name + "!");
                return null;
            }
            return Accessors[scene].Get<T>();
        }

        public static T GetFromActiveScene<T>() where T : ReferableMonoBehaviour => Get<T>(SceneManager.GetActiveScene());

        protected virtual void Awake()
        {
            if (Accessor.HasReferenceOfType(this))
            {
                Debug.LogError(GetType().Name + " is already registered for scene: " + gameObject.scene.name + "!");
            }
            else
            {
                Accessor.Register(this);
                Debug.Log(GetType().Name + " is registered for scene: " + gameObject.scene.name + ".");
            }
        }

        protected virtual void OnDestroy()
        {
            Accessor.Remove(this);
        }

        static Dictionary<Scene, ReferableMonoBehaviourAccessor> Accessors = new Dictionary<Scene, ReferableMonoBehaviourAccessor>();
        ReferableMonoBehaviourAccessor Accessor
        {
            get
            {
                if (!Accessors.ContainsKey(gameObject.scene))
                {
                    Accessors.Add(gameObject.scene, new ReferableMonoBehaviourAccessor());
                }
                return Accessors[gameObject.scene];
            }
        }

        class ReferableMonoBehaviourAccessor
        {
            HashSet<ReferableMonoBehaviour> _Behaviors = new HashSet<ReferableMonoBehaviour>();

            public void Register<T>(T behavior) where T : ReferableMonoBehaviour => _Behaviors.Add(behavior);
            public void Remove<T>(T behavior) where T : ReferableMonoBehaviour => _Behaviors.Remove(behavior);
            public T Get<T>() where T : ReferableMonoBehaviour => _Behaviors.FirstOrDefault(_ => _ is T) as T;
            public bool HasReferenceOfType<T>(T target) where T : ReferableMonoBehaviour => Get<T>();
        }
    }
}
