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

        private TerrainModelProductMetadata GenerateTerrainModelProductMetadata(string productId, int width, int height) {
            return new TerrainModelProductMetadata(productId, UnrestrictedBoundingBox.Global, width, height);
        }

    }

}
