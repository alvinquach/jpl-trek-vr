using System;
using System.IO;
using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractableGlobeTerrain))]
    public sealed class GlobeTerrainModel : TerrainModel {

        public override string DemUUID {
            get => GlobalDigitalElevationModelUUID;
            set { /* Do nothing */ }
        }

        public override XRInteractableTerrain InteractionController => GetComponent<XRInteractableGlobeTerrain>();

        public event Action OnInitComplete = () => { };

        public GlobeTerrainModel() {
            BaseMosaicProduct = GenerateProductMetadata(GlobalMosaicUUID, 0, 0);
        }

        protected override void GenerateMaterial() {
            base.GenerateMaterial();

            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;
            textureManager.GetTexture(BaseMosaicProduct, texture => {
                Material.SetTexture("_DiffuseBase", texture); // Assume Material is not null or default.
            });
        }

        protected override void GenerateMesh() {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();

            // TEMPORARY -- DO THIS PROPERLY
            string demFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.DigitalElevationModel,
                TerrainModelManager.Instance.GlobalDEMFilepath
            );

            GenerateTerrainMeshTask generateMeshTask = 
                new GenerateGlobeTerrainMeshFromDigitalElevationModelTask(demFilePath, metadata);

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

        private TerrainModelProductMetadata GenerateProductMetadata(string productId, int width, int height) {
            return new TerrainModelProductMetadata(productId, UnrestrictedBoundingBox.Global, width, height);
        }

    }

}
