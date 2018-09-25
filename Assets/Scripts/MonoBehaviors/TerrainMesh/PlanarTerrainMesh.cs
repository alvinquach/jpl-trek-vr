using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class PlanarTerrainMesh : TerrainMesh {

    private TiffTerrainMeshGenerator _meshGenerator;

    protected override TiffTerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator == null) {
                _meshGenerator = new TiffPlanarTerrainMeshGenerator(_demFilePath);
            }
            return _meshGenerator;
        }
    }

    protected override void ProcessMeshData() {
        base.ProcessMeshData();

        // TEMPORARY
        transform.localPosition = new Vector3(0, 0.31f, 0);
    }

}
