using UnityEngine;

/// <summary>
///     Generates the mesh for a partial section of the planet defined
///     by bounding box coordinates. Uses TIFF DEM files as a source of
///     height data.
/// </summary>
public class DigitalElevationModelPartialTerrainMeshGenerator : BasePartialTerrainMeshGenerator {

    public DigitalElevationModelPartialTerrainMeshGenerator(float radius, Vector4 boundingBox) :
        base(radius, boundingBox) {

    }

}
