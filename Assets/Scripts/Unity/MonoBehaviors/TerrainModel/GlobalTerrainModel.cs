using UnityEngine;

public class GlobalTerrainModel : TerrainModel {

    [SerializeField]
    private float _radius;

    public float Radius {
        get { return _radius; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value; }
    }

    protected override void ProcessMeshData(MeshData[] meshData) {
        base.ProcessMeshData(meshData);

        // Add a sphere collider to the mesh, so that it can be manipulated using the controller.
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = _radius;
    }

    protected override GenerateTerrainMeshTask InstantiateGenerateMeshTask() {
        TerrainModelMetadata metadata = GenerateMetadata();
        return new GenerateDigitalElevationModelSphericalTerrainMeshTask(metadata);
    }

    protected override TerrainModelMetadata GenerateMetadata() {
        return new TerrainModelMetadata() {
            demFilePath = _demFilePath,
            albedoFilePath = _albedoFilePath,
            radius = _radius,
            heightScale = _heightScale,
            lodLevels = _lodLevels,
            baseDownsample = _baseDownsampleLevel
        };
    }
}
