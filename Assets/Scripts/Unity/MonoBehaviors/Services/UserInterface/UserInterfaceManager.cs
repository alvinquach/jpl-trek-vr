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

        [SerializeField]
        private PrimaryXRController _primaryController;

        [SerializeField]
        private SecondaryXRController _secondaryController;

        #region BrowserUserInterface instances

        public MainModal MainModal { get; private set; }

        public ControllerModal PrimaryControllerModal { get; private set; }

        public ControllerModal SecondaryControllerModal { get; private set; }

        #endregion

        void Awake() {

            Debug.Log("UserInterfaceManager Awake");

            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                // TODO Throw exception
            }

            GameObject gameObject = new GameObject();
            gameObject.name = typeof(MainModal).Name;
            MainModal = gameObject.AddComponent<MainModal>();

            if (_primaryController) {
                gameObject = new GameObject();
                gameObject.name = typeof(ControllerModal).Name;
                gameObject.transform.parent = _primaryController.transform;
                PrimaryControllerModal = gameObject.AddComponent<ControllerModal>();
            }

            if (_secondaryController) {
                gameObject = new GameObject();
                gameObject.name = typeof(ControllerModal).Name;
                gameObject.transform.parent = _secondaryController.transform;
                SecondaryControllerModal = gameObject.AddComponent<ControllerModal>();
            }

        }

    }

}
