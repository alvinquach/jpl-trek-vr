using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TerrainModelGlobalLayerController : TerrainModelLayerController {

        private bool _globalLayersChanged;

        public override IList<TerrainModelLayer> Layers => _terrainModelManager.GlobalLayers;

        #region Unity lifecycle methods

        private void Awake() {
            _terrainModelManager.OnGlobalLayersChanged += GlobalLayersChanged;
        }

        private void OnEnable() {
            if (_globalLayersChanged) {
                ProcessGlobalLayerChange();
                _globalLayersChanged = false;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _terrainModelManager.OnGlobalLayersChanged -= GlobalLayersChanged;
        }

        #endregion

        public override void AddLayer(string productUUID, Action callback = null, int ? index = null) {

            // Check if the product has already been added.
            IList<TerrainModelLayer> layers = Layers;
            if (layers.Any(l => l.ProductUUID == productUUID)) {
                Debug.LogWarning($"{productUUID} has already been added as a layer.");
                return;
            }

            CreateLayer(productUUID, layer => {
                if (_terrainModelManager.AddGlobalLayer(layer, index)) {
                    ReloadTextures(Layers);
                }
                callback?.Invoke();
            });
        }

        public override void UpdateLayer(TerrainModelLayerChange changes, Action callback = null) {
            if (_terrainModelManager.UpdateGlobalLayer(changes)) {
                // TODO Update material here
            }
            callback?.Invoke();
        }

        public override void RemoveLayer(int index, Action callback = null) {
            if (_terrainModelManager.RemoveGlobalLayer(index)) {
                ReloadTextures(Layers);
            }
            callback?.Invoke();
        }

        protected override Material GenerateMaterial() {
            return new Material(_terrainModelManager.BaseMaterial);
        }

        private void GlobalLayersChanged() {
            // If the terrain model is not visible, then apply the changes later.
            if (!isActiveAndEnabled) {
                _globalLayersChanged = true;
            }

            // If the terrain model is visible, then its terrain model controller should
            // be the one sending the changes, so it should already have the changes.
        }

        private void ProcessGlobalLayerChange() {

        }

    }

}