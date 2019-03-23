using System;
using UnityEngine;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractableGlobe))]
    public class GlobeTerrainModel : TerrainModel {

        public const float GlobeModelScale = 2.5e-7f;

        private float _radius;
        public float Radius {
            get { return _radius * GlobeModelScale; }
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _radius = value;
                }
            }
        }

        public event Action OnInitComplete = () => { };

        protected override void GenerateMaterial() {
            base.GenerateMaterial();

            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;
            textureManager.GetGlobalMosaicTexture(texture => {
                Material.SetTexture("_DiffuseBase", texture); // Assume Material is not null or default.
            });
        }

        protected override void GenerateMesh() {
            TerrainModelMetadata metadata = GenerateTerrainModelMetadata();
            GenerateTerrainMeshTask generateMeshTask = new GenerateGlobeTerrainMeshFromDigitalElevationModelTask(metadata);
            generateMeshTask.Execute((meshData) => {
                QueueTask(() => {
                    ProcessMeshData(meshData);
                    _initTaskStatus = TaskStatus.Completed;
                    OnInitComplete.Invoke();
                });
            });
        }

        protected override void ProcessMeshData(MeshData[] meshData) {
            base.ProcessMeshData(meshData);

            // Adds a sphere collider to the mesh, so that it can be manipulated using the controller.
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = Radius;
        }

        protected override TerrainModelMetadata GenerateTerrainModelMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                radius = Radius,
                heightScale = HeightScale * GlobeModelScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

        protected override void SetRenderMode(bool enabled) {
            base.SetRenderMode(enabled);
            if (_initTaskStatus == TaskStatus.Completed) {
                if (!enabled) {
                    XRInteractableGlobe planet = GetComponent<XRInteractableGlobe>();
                    planet.SwitchToMode(XRInteractableGlobeMode.Disabled);
                }
                else {
                    // TODO ...
                }
            }
        }

        private TerrainModelProductMetadata GenerateTerrainModelProductMetadata(string productId, int width, int height) {
            return new TerrainModelProductMetadata(productId, UnrestrictedBoundingBox.Global, width, height);
        }

    }

}
