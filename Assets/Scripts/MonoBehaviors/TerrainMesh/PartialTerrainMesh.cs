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

    private TerrainMeshGenerator _meshGenerator;

    protected override TerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator == null) {
                _meshGenerator = new PartialTerrainMeshGenerator(
                    _radius,
                    _boundingBox
                );
            }
            return _meshGenerator;
        }
    }

}