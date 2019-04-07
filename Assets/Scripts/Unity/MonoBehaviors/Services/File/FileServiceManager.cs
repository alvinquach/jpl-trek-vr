using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class FileServiceManager : MonoBehaviour {

        /// <summary>
        ///     The instance of the FileServiceManager that is present in the scene.
        ///     There should only be one FileServiceManager in the entire scene.
        /// </summary>
        public static FileServiceManager Instance { get; private set; }

        public FileServiceManager() {
            if (!Instance) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
                throw new Exception($"Only one instance of {GetType().Name} is allowed.");
            }
        }

    }

}
