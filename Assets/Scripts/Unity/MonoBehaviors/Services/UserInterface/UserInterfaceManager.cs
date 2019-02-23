using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class UserInterfaceManager : MonoBehaviour {

        /// <summary>
        ///     The instance of the UserInterfaceManager that is present in the scene.
        ///     There should only be one TerrainModelManager in the entire scene.
        /// </summary>
        public static UserInterfaceManager Instance { get; private set; }

        [SerializeField]
        private Material _material;
        public Material UIMaterial {
            get { return _material; }
        }

        public MainModal MainModal { get; private set; }

        void Awake() {

            Debug.Log("UserInterfaceManager Awake");

            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                // TODO Throw exception
            }

            GameObject menu = new GameObject();
            menu.name = typeof(MainModal).Name;
            MainModal = menu.AddComponent<MainModal>();

        }

    }

}
