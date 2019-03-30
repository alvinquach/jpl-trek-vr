using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class TerrainControlPanel : XRBrowserUserInterface {

        /// <summary>
        ///     Width of the modal in world units.
        /// </summary>
        private const float WorldWidth = 0.9f;

        /// <summary>
        ///     Height of the modal in world units.
        /// </summary>
        private const float WorldHeight = 0.45f;

        /// <summary>
        ///     Vertical resolution of the screen in pixels.
        /// </summary>
        private const int Resolution = 1024;

        protected override int Width => Mathf.RoundToInt(WorldWidth / WorldHeight * Resolution);

        protected override int Height => Resolution;

        protected override bool HideAfterInit => false;

        private UnityBrowserTerrainModelFunctions _terrainModelFunctions;

        private readonly GeneratePlanarMenuMeshTask _generateMenuMeshTask =
            new GeneratePlanarMenuMeshTask(WorldWidth, WorldHeight, RelativePosition.Center);
        protected override GenerateMenuMeshTask GenerateMenuMeshTask => _generateMenuMeshTask;

        protected override string DefaultUrl => $"{BaseUrl}#{TerrainControlPanelUrl}";

        protected override void Init(Mesh mesh) {
            base.Init(mesh);
            _terrainModelFunctions = new UnityBrowserTerrainModelFunctions(Browser);
        }

        protected override void OnBrowserLoad(JSONNode loadData) {
            _terrainModelFunctions.RegisterFunctions();
        }

    }

}
