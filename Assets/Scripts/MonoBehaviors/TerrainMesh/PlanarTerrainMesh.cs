using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class PlanarTerrainMesh : TerrainMesh {

    [SerializeField]
    private float _size;

    public float Size {
        get { return _size; }
        set { if (!_initStarted) _size = value; }
    }

    private TerrainMeshGenerator _meshGenerator;

    protected override TerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator == null) {
                _meshGenerator = new TiffPlanarTerrainMeshGenerator(
                    _demFilePath,
                    _size,
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

        // TEMPORARY
        transform.localPosition = new Vector3(0, 0.31f, 0);
    }

}
