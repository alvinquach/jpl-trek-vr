using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class SphericalTerrainMesh : TerrainMesh {

    [SerializeField]
    private float _radius;

    public float Radius {
        get { return _radius; }
        set { if (!_initStarted) _radius = value; }
    }

    private TiffTerrainMeshGenerator _meshGenerator;

    protected override TiffTerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator == null) {
                _meshGenerator = new TiffSphericalTerrainMeshGenerator(
                    _demFilePath,
                    _radius,
                    _heightScale,
                    _lodLevels,
                    _baseDownsampleLevel
                );
            }
            return _meshGenerator;
        }
    }

    protected override void ProcessMeshData() {
        base.ProcessMeshData();

        // Add a sphere collider to the mesh, so that it can be manipulated using the controller.
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = _radius;
    }

}
