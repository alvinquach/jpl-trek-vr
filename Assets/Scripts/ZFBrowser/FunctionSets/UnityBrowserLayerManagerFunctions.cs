using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityBrowserLayerManagerFunctions : UnityBrowserFunctionSet {

        private readonly TerrainModelManager _terrainModelManager = TerrainModelManager.Instance;

        protected override string FunctionsReadyVariable => "layerFunctionsReady";

        private TerrainLayerController CurrentLayerController {
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
            TerrainLayerChange changes = 
                JsonConvert.DeserializeObject<TerrainLayerChange>(layerChangeJson, JsonConfig.SerializerSettings);

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
            IList<TerrainLayer> layers = _terrainModelManager.CurrentVisibleModel.LayerController.Layers;
            ZFBrowserUtils.SendDataResponse(_browser, requestId, layers);
        }

        private void SendLayersToBrowser() {
            IList<TerrainLayer> layers = CurrentLayerController.Layers;
            string response = JsonConvert.SerializeObject(layers, JsonConfig.SerializerSettings);
            Debug.Log(response);
            _browser.EvalJS($"{UnityGlobalObjectPath}.onLayersUpdated.next({response});");
        }

    }

}
