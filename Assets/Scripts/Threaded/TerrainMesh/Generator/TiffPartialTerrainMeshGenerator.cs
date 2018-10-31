using UnityEngine;

/// <summary>
///     Generates the mesh for a partial section of the planet defined
///     by bounding box coordinates. Uses TIFF DEM files as a source of
///     height data.
/// </summary>
public class TiffPartialTerrainMeshGenerator : BasePartialTerrainMeshGenerator {

    public TiffPartialTerrainMeshGenerator(float radius, Vector4 boundingBox) :
        base(radius, boundingBox) {

    }

}
