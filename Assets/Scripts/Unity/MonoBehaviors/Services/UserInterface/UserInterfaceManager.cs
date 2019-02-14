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
        private Browser _browser;

        void Awake() {

            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                // TODO Throw exception
            }

            //if (_browser) {
            //    _browser.UIHandler = new BrowserInputHandler();
            //}

        }

    }

}
