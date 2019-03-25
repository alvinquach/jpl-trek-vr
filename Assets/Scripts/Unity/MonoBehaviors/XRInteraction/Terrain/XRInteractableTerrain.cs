using UnityEngine;
using System.Collections;

namespace TrekVRApplication {

    public abstract class XRInteractableTerrain : XRInteractableObject {

        public abstract TerrainModel TerrainModel { get; }

        protected virtual void Awake() {
            TerrainModelManager.Instance.OnInteractionStatusChange += EnableTerrainInteraction;
        }
        protected virtual void OnDestroy() {
            TerrainModelManager.Instance.OnInteractionStatusChange -= EnableTerrainInteraction;
        }

        protected virtual void EnableTerrainInteraction(bool enabled) {
            TerrainModel.UseDisabledMaterial = !enabled;
        }

    }

}
