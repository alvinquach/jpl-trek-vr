using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityBrowserLayerManagerFunctions : UnityBrowserFunctionSet {

        private readonly TerrainModelManager _terrainModelManager = TerrainModelManager.Instance;

        protected override string FunctionsReadyVariable => "layerFunctionsReady";

        private TerrainModelLayerController CurrentLayerController {
            get => _terrainModelManager.CurrentVisibleModel.LayerController;
        }

        public UnityBrowserLayerManagerFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void AddLayer(string uuid, double? index) {
            CurrentLayerController.AddLayer(uuid, (int?)index, () => SendLayersToBrowser());
        }

        [RegisterToBrowser]
        public void UpdateLayer(string layerChangeJson) {
            TerrainModelLayerChange changes = 
                JsonConvert.DeserializeObject<TerrainModelLayerChange>(layerChangeJson, JsonConfig.SerializerSettings);

            CurrentLayerController.UpdateLayer(changes);
        }

        [RegisterToBrowser]
        public void MoveLayer(double? from, double? to) {
            if (from == null || to == null) {
                return;
            }
            CurrentLayerController.MoveLayer((int)from, (int)to, () => SendLayersToBrowser());
        }

        [RegisterToBrowser]
        public void RemoveLayer(double? index) {
            if (index == null) {
                return;
            }
            CurrentLayerController.RemoveLayer((int)index, () => SendLayersToBrowser());
        }


        [RegisterToBrowser]
        public void GetCurrentLayers(string requestId) {
            IList<TerrainModelLayer> layers = _terrainModelManager.CurrentVisibleModel.LayerController.Layers;
            ZFBrowserUtils.SendDataResponse(_browser, requestId, layers);
        }

        private void SendLayersToBrowser() {
            IList<TerrainModelLayer> layers = CurrentLayerController.Layers;
            string response = JsonConvert.SerializeObject(layers, JsonConfig.SerializerSettings);
            Debug.Log(response);
            _browser.EvalJS($"{UnityGlobalObjectPath}.onLayersUpdated.next({response});");
        }

    }

}
