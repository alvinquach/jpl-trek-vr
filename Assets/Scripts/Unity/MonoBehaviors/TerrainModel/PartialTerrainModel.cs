using UnityEngine;

public class PartialTerrainModel : TerrainModelBase {

    [SerializeField]
    private float _radius;

    public float Radius {
        get { return _radius; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value; }
    }

    [SerializeField]
    private Vector4 _boundingBox;

    public Vector4 BoundingBox {
        get { return _boundingBox; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _boundingBox = value; }
    }

    private TaskStatus _viewTransitionTaskStatus = TaskStatus.NotStarted;
    private float _viewTransitionProgress = 0.0f;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;
    private float _targetScale;


    /// <summary>
    ///     For generating a mesh without any height data. The gernated mesh will be
    ///     temporarily displayed while the DEM and textures are retrieved, and a new
    ///     mesh with height data is generated.
    /// </summary>
    private BasePartialTerrainMeshGenerator _baseMeshGenerator;

    /// <summary>
    ///     For generating generates the mesh with height data.
    /// </summary>
    private DigitalElevationModelPartialTerrainMeshGenerator _meshGenerator;

    protected override TerrainMeshGenerator MeshGenerator {
        get {
            if (_meshGenerator) {
                return _meshGenerator;
            }
            else if (!_baseMeshGenerator && _initTaskStatus > TaskStatus.NotStarted) {
                _baseMeshGenerator = new BasePartialTerrainMeshGenerator(
                    _radius,
                    _boundingBox
                );
            }
            return _baseMeshGenerator;
        }
    }

    protected override void Start() {
        base.Start();
        // TODO Call method to download DEM and textures here.
    }

    protected override void Update() {

        if (_initTaskStatus == TaskStatus.Started && _baseMeshGenerator.Complete) {
            ProcessMeshData(_baseMeshGenerator);

            transform.rotation = TerrainModelManager.Instance.GetDefaultPlanetModelTransform().rotation;
            transform.localPosition = transform.rotation * (0.25f * 3.39f * BoundingBoxUtils.MedianDirection(_boundingBox));

            _initTaskStatus = TaskStatus.Completed;

            // Start the view transition.
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _targetRotation = Quaternion.FromToRotation(BoundingBoxUtils.MedianDirection(_boundingBox), Vector3.up);
            _targetScale = 0.5f / BoundingBoxUtils.LargestDimension(_boundingBox);
            _viewTransitionTaskStatus = TaskStatus.Started;
        }

        if (_viewTransitionTaskStatus == TaskStatus.Started) {
            // TODO Un-hardcode these values
            _viewTransitionProgress += Time.deltaTime / 1.337f;
            transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _viewTransitionProgress);
            transform.position = Vector3.Lerp(_startPosition, 0.69f * Vector3.up, _viewTransitionProgress);
            transform.localScale = Vector3.Lerp(0.25f * Vector3.one, _targetScale * Vector3.one, _viewTransitionProgress);
            if (_viewTransitionProgress >= 1.0f) {
                _viewTransitionTaskStatus = TaskStatus.Completed;
            } 
        }

    }


}