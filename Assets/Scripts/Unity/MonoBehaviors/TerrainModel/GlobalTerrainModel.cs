using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class GlobalTerrainModel : TerrainModel {

    [SerializeField]
    private float _radius;

    public float Radius {
        get { return _radius; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value; }
    }

    private DigitalElevationModelSphericalTerrainMeshGenerator _meshGenerator;

    protected override TerrainMeshGenerator MeshGenerator {
        get {
            if (!_meshGenerator && _initTaskStatus > TaskStatus.NotStarted) {
                _meshGenerator = new DigitalElevationModelSphericalTerrainMeshGenerator(
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

    protected override void ProcessMeshData(TerrainMeshGenerator meshGenerator = null) {
        base.ProcessMeshData(meshGenerator);

        // Add a sphere collider to the mesh, so that it can be manipulated using the controller.
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = _radius;
    }

}
