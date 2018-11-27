using App.Geo;

/// <summary>
///     Generates the mesh for a partial section of the planet defined
///     by bounding box coordinates. Uses TIFF DEM files as a source of
///     height data.
/// </summary>
public class GenerateDigitalElevationModelPartialTerrainMeshTask : GenerateDigitalElevationModelTerrainMeshTask {

    protected BoundingBox _boundingBox;

    public GenerateDigitalElevationModelPartialTerrainMeshTask(TerrainModelMetadata metadata, BoundingBox boundingBox) : base(metadata) {
        _boundingBox = boundingBox;
    }

    protected override MeshData GenerateForLod(Image<float> image, int downsample) {
        // TODO Implement this
        throw new System.NotImplementedException();
    }

}
