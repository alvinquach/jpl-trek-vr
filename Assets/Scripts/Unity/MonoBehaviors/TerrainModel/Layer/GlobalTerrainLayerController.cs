using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class GlobalTerrainLayerController : TerrainLayerController {

        private bool _globalLayersChanged;

        public override IList<TerrainLayer> Layers => _terrainModelManager.GlobalLayers;

        #region Unity lifecycle methods

        private void Awake() {
            _terrainModelManager.OnGlobalLayersChanged += GlobalLayersChanged;
        }

        private void OnEnable() {
            if (_globalLayersChanged) {
                ReloadTextures(Layers); // TODO Compare changes properly
                _globalLayersChanged = false;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _terrainModelManager.OnGlobalLayersChanged -= GlobalLayersChanged;
        }

        #endregion

        public override void AddLayer(string productUUID, int ? index = null, Action callback = null) {
            IList<TerrainLayer> layers = Layers;

            // Check if the number of layers is already maxed out.
            if (layers.Count >= MaxDiffuseLayers) {
                Debug.LogError($"Number of layers cannot exceed {MaxDiffuseLayers}.");
                return;
            }

            // Check if the product has already been added.
            if (layers.Any(l => l.ProductUUID == productUUID)) {
                Debug.LogError($"{productUUID} has already been added as a layer.");
                return;
            }

            CreateLayer(productUUID, layer => {
                if (_terrainModelManager.AddGlobalLayer(layer, index)) {
                    ReloadTextures(Layers, false);
                }
                callback?.Invoke();
            });
        }

        public override void UpdateLayer(TerrainLayerChange changes, Action callback = null) {
            if (_terrainModelManager.UpdateGlobalLayer(changes)) {
                int index = changes.Index;
                TerrainLayer layer = Layers[index];
                Material.SetFloat($"_Diffuse{index}Opacity", layer.Visible ? layer.Opacity : 0);
            }
            callback?.Invoke();
        }

        public override void MoveLayer(int from, int to, Action callback = null) {
            if (_terrainModelManager.MoveGlobalLayer(from, to)) {
                ReloadTextures(Layers, false);
            }
            callback?.Invoke();
        }

        public override void RemoveLayer(int index, Action callback = null) {
            if (_terrainModelManager.RemoveGlobalLayer(index)) {
                ReloadTextures(Layers, false);
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

    }

}