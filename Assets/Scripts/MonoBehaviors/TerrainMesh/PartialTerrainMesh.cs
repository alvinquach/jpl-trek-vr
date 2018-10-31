using UnityEngine;

public class PartialTerrainMesh : TerrainMesh {

    [SerializeField]
    private float _radius;

    public float Radius {
        get { return _radius; }
        set { if (!_initStarted) _radius = value; }
    }

    [SerializeField]
    private Vector4 _boundingBox;

    public Vector4 BoundingBox {
        get { return _boundingBox; }
        set { if (!_initStarted) _boundingBox = value; }
    }

    /// <summary>
    ///     For generating a mesh without any height data. The gernated mesh will be
    ///     temporarily displayed while the DEM and textures are retrieved, and a new
    ///     mesh with height data is generated.
    /// </summary>
    private BasePartialTerrainMeshGenerator _baseMeshGenerator;

    /// <summary>
    ///     For generating generates the mesh with height data.
    /// </summary>
    private TiffPartialTerrainMeshGenerator _meshGenerator;

    protected override TerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator) {
                return _meshGenerator;
            }
            else if (!_baseMeshGenerator && _initStarted) {
                _baseMeshGenerator = new BasePartialTerrainMeshGenerator(
                    _radius,
                    _boundingBox
                );
            }
            return _baseMeshGenerator;
        }
    }

    private float _tabletopTransitionProgress = 0.0f;

    protected override void Start() {
        base.Start();
        // TODO Call method to download DEM and textures here.
    }

    protected override void Update() {
        if (_initStarted && !_initCompleted && _baseMeshGenerator.Complete) {
            ProcessMeshData(_baseMeshGenerator);
            _initCompleted = true;
        }
    }

}