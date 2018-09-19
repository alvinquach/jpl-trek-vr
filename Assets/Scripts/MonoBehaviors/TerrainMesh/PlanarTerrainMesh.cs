using UnityEngine;

public class PlanarTerrainMesh : TerrainMesh {

    public override TerrainGeometryType SurfaceGeometryType {
        get { return TerrainGeometryType.Planar; }
    }

    public override void InitMesh() {
        base.InitMesh();

        // TEMPORARY
        transform.localPosition = new Vector3(0, 0.1f, 0);
    }

}
