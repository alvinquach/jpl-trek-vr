using UnityEngine;
using System.Collections;
using System;

namespace TrekVRApplication {

    /// <summary>
    ///     <para>
    ///         Enforces a singleton pattern in the implementing class.
    ///     </para>
    ///     <para>
    ///         Note that classes with multiple levels of inheritance
    ///         doesn't work well with the generics required by this
    ///         class; for such classes, it is recommended to manually
    ///         add the singleton logic instead.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">The type (class name) of the implementing class.</typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T> {

        public static T Instance { get; private set; }

        public SingletonMonoBehaviour() {
            if (!Instance) {
                Instance = (T)this;
            }
            else if (Instance != this) {
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }
        }

        protected virtual void Awake() {
            if (Instance != this) {
                Destroy(this);
            }
        }

        protected virtual void OnDestroy() {
            Instance = null;
        }

    }

}
