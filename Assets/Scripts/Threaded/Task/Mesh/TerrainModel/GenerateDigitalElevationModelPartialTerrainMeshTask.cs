using UnityEngine;

/// <summary>
///     Generates the mesh for a partial section of the planet defined
///     by bounding box coordinates. Uses TIFF DEM files as a source of
///     height data.
/// </summary>
public class GenerateDigitalElevationModelPartialTerrainMeshTask : GenerateDigitalElevationModelTerrainMeshTask {

    protected Vector4 _boundingBox;

    public GenerateDigitalElevationModelPartialTerrainMeshTask(TerrainModelMetadata metadata, Vector4 boundingBox) : base(metadata) {
        _boundingBox = boundingBox;
    }

    protected override MeshData GenerateForLod(Image<float> image, int downsample) {
        // TODO Implement this
        throw new System.NotImplementedException();
    }

}
