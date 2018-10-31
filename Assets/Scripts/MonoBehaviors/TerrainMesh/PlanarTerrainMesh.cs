using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class PlanarTerrainMesh : TerrainMesh {

    [SerializeField]
    private float _size;

    public float Size {
        get { return _size; }
        set { if (!_initStarted) _size = value; }
    }

    private TiffPlanarTerrainMeshGenerator _meshGenerator;

    protected override TerrainMeshGenerator MeshGenerator {
        get {
            if (!_meshGenerator && _initStarted) {
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

    protected override void ProcessMeshData(TerrainMeshGenerator meshGenerator = null) {
        base.ProcessMeshData(meshGenerator);

        // TEMPORARY
        transform.localPosition = new Vector3(0, 0.31f, 0);
    }

}
