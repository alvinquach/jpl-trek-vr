using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class SphericalTerrainMesh : TerrainMesh {

    private TiffTerrainMeshGenerator _meshGenerator;

    protected override TiffTerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator == null) {
                _meshGenerator = new TiffSphericalTerrainMeshGenerator(_demFilePath);
            }
            return _meshGenerator;
        }
    }

    protected override void ProcessMeshData() {
        base.ProcessMeshData();

        // Add a sphere collider to the mesh, so that it can be manipulated using the controller.
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = _scale;
    }

}
