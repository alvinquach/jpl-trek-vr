using System;
using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractableGlobe))]
    public class GlobeTerrainModel : TerrainModel {

        public override XRInteractableTerrain InteractionController => GetComponent<XRInteractableGlobe>();

        public event Action OnInitComplete = () => { };

        protected override void GenerateMaterial() {
            base.GenerateMaterial();

            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;
            textureManager.GetGlobalMosaicTexture(texture => {
                Material.SetTexture("_DiffuseBase", texture); // Assume Material is not null or default.
            });
        }

        protected override void GenerateMesh() {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();
            GenerateTerrainMeshTask generateMeshTask = new GenerateGlobeTerrainMeshFromDigitalElevationModelTask(metadata);
            generateMeshTask.Execute(meshData => {
                _referenceMeshData = meshData;
                QueueTask(() => {
                    ProcessMeshData(_referenceMeshData);
                    _initTaskStatus = TaskStatus.Completed;
                    OnInitComplete.Invoke();
                });
            });
        }

        protected override void ProcessMeshData(MeshData[] meshData) {
            base.ProcessMeshData(meshData);

            float radius = Radius * TerrainModelScale;

            // Adds a sphere collider to the mesh, so that it can be manipulated using the controller.
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = radius;

            // Add a sphere to cast shadows
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh mesh = sphere.GetComponent<MeshFilter>().mesh;
            GameObject shadowCaster = AddShadowCaster(mesh);
            shadowCaster.transform.localScale = 2 * radius * Vector3.one;
            Destroy(sphere);
        }

        protected override bool CanRescaleTerrainHeight() {
            return isActiveAndEnabled
                && _initTaskStatus == TaskStatus.Completed 
                && _heightRescaleTaskStatus != TaskStatus.InProgress;
        }

        protected override void RescaleTerrainHeight(float scale) {
            RescaleTerrainMeshHeightTask rescaleMeshHeightTask = 
                new RescaleGlobeTerrainMeshHeightTask(_referenceMeshData, GenerateMeshMetadata());
            _heightRescaleTaskStatus = TaskStatus.InProgress;
            rescaleMeshHeightTask.Execute(rescaledMeshData => {
                QueueTask(() => {
                    ApplyRescaledMeshData(rescaledMeshData);
                    _heightRescaleTaskStatus = TaskStatus.Completed;
                });
            });
        }

        private TerrainModelProductMetadata GenerateTerrainModelProductMetadata(string productId, int width, int height) {
            return new TerrainModelProductMetadata(productId, UnrestrictedBoundingBox.Global, width, height);
        }

    }

}
