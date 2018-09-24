using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class PlanarTerrainMesh : TerrainMesh {

    public override TerrainGeometryType SurfaceGeometryType {
        get { return TerrainGeometryType.Planar; }
    }

    public override void InitMesh() {
        base.InitMesh();

        // TEMPORARY
        transform.localPosition = new Vector3(0, 0.31f, 0);
    }

    protected override Mesh GenerateMesh(TiffTerrainMeshGenerator meshGenerator, int downsample) {
        return meshGenerator.GeneratePlanarMesh(_scale, _heightScale, downsample);
    }

}
