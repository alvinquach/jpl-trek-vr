using System;
using System.IO;
using UnityEngine;
using static TrekVRApplication.TerrainConstants;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractableGlobeTerrain))]
    public sealed class GlobeTerrainModel : TerrainModel {

        public override string DemUUID {
            get => GlobalDigitalElevationModelUUID;
            set { /* Do nothing */ }
        }

        public override XRInteractableTerrain InteractionController => GetComponent<XRInteractableGlobeTerrain>();

        public event Action OnInitComplete = () => { };

        protected override void AddRenderTextureOverlay() {
            TerrainOverlayController overlayController = GlobeTerrainOverlayController.Instance;
            if (overlayController) {
                LayerController.Material.SetTexture("_Overlay", overlayController.RenderTexture);
            }
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
                new GenerateGlobeTerrainMeshFromDigitalElevationModelTask(new string[] { demFilePath }, metadata);

            generateMeshTask.Execute(meshData => {
                _referenceMeshData = meshData;
                QueueTask(() => {
                    ProcessMeshData(_referenceMeshData);
                    PostProcessMeshData(_referenceMeshData, metadata);
                    _initTaskStatus = TaskStatus.Completed;
                    OnInitComplete.Invoke();
                });
            });
        }

        private void PostProcessMeshData(MeshData[] meshData, TerrainModelMeshMetadata metadata) {

            float radius = Radius * TerrainModelScale;

            // Add mesh collider, if a physics mesh was generated.
            int physicsMeshIndex = metadata.PhyiscsLodMeshIndex;
            if (physicsMeshIndex < 0) {
                return;
            }
            Mesh physicsMesh = ConvertToMesh(meshData[physicsMeshIndex], false);

            // Do the heavy processing in the next update (does this help with the stutter?).
            QueueTask(() => {
                MeshCollider collider = gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = physicsMesh;
            });

            // Use the lowest LOD as a shadow caster.
            Mesh shadowMesh;
            if (physicsMeshIndex == meshData.Length - 1) {
                shadowMesh = physicsMesh;
            } else {
                Transform child = _lodGroupContainer.transform.Find($"LOD_{_lodLevels}");
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                shadowMesh = meshFilter.mesh;
            }
            GameObject shadowCaster = AddShadowCaster(shadowMesh);
        }


        protected override bool CanRescaleTerrainHeight() {
            return isActiveAndEnabled
                && _initTaskStatus == TaskStatus.Completed 
                && _heightRescaleTaskStatus != TaskStatus.InProgress;
        }

        protected override void RescaleTerrainHeight(float scale) {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();
            RescaleTerrainMeshHeightTask rescaleMeshHeightTask = 
                new RescaleGlobeTerrainMeshHeightTask(_referenceMeshData, GenerateMeshMetadata());
            _heightRescaleTaskStatus = TaskStatus.InProgress;
            rescaleMeshHeightTask.Execute(rescaledMeshData => {
                QueueTask(() => {
                    ApplyRescaledMeshData(rescaledMeshData);

                    // Update the physics mesh if applicable.
                    int physicsMeshIndex = metadata.PhyiscsLodMeshIndex;
                    if (physicsMeshIndex >= 0) {
                        _physicsMeshUpdateTimer = PhysicsMeshUpdateDelay;
                        _physicsMeshUpdatedData = rescaledMeshData[physicsMeshIndex];
                    }

                    _heightRescaleTaskStatus = TaskStatus.Completed;
                });
            });
        }

        private TerrainProductMetadata GenerateProductMetadata(string productId, int width, int height) {
            return new TerrainProductMetadata(productId, UnrestrictedBoundingBox.Global, width, height);
        }

    }

}
