using App.Texture.Models;
using App.Unity.MonoBehaviors;
using System;
using UnityEngine;

public abstract class TerrainModel : MonoBehaviourWithTaskQueue<MeshData[]> {

    [SerializeField]
    protected string _demFilePath;

    public string DemFilePath {
        get { return _demFilePath; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _demFilePath = value; }
    }

    [SerializeField]
    protected string _albedoFilePath;

    public string AlbedoFilePath {
        get { return _albedoFilePath; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _albedoFilePath = value; }
    }

    [SerializeField]
    protected float _heightScale = 1.0f;

    public float HeightScale {
        get { return _heightScale; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _heightScale = value; }
    }

    [SerializeField]
    protected int _baseDownsampleLevel = 2;

    public int BaseDownSampleLevel {
        get { return _baseDownsampleLevel; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _baseDownsampleLevel = value; }
    }

    // TODO Add option to use linear LOD downsampling.

    [SerializeField]
    protected int _lodLevels = 2;

    public int LodLevels {
        get { return _lodLevels; }
        set { if (_initTaskStatus == TaskStatus.NotStarted) _lodLevels = value; }
    }

    public Material Material { get; set; }

    protected TaskStatus _initTaskStatus = TaskStatus.NotStarted;

    /// <summary>
    ///     The last time this terrain was set to visible via the Visible property.
    /// </summary>
    public long LastVisible { get; private set; } = 0L;

    public bool Visible {
        get {
            return gameObject.activeSelf;
        }
        set {
            if (!value) {
                gameObject.SetActive(false);
            }
            else if (!gameObject.activeSelf) {
                LastVisible = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                gameObject.SetActive(true);
            }
        }
    }

    #region Unity lifecycle methods

    protected virtual void Start() {
        InitModel();
    }

    // If this method is overriden, then this method should be 
    // called from the overriding method.
    protected override void Update() {
        base.Update();
    }

    #endregion

    // Can only be called once.
    public virtual void InitModel() {
        if (_initTaskStatus > TaskStatus.NotStarted) {
            return;
        }
        _initTaskStatus = TaskStatus.Started;
        GenerateTerrainMeshTask generateMeshTask = InstantiateGenerateMeshTask();
        generateMeshTask.Start((meshData) => {
            QueueTask(ProcessMeshData, meshData);
            _initTaskStatus = TaskStatus.Completed;
        });
    }

    /// <summary>Instantiates the task for generating the terrain model mesh data.</summary>
    protected abstract GenerateTerrainMeshTask InstantiateGenerateMeshTask();

    protected virtual void ProcessMeshData(MeshData[] meshData) {

        // If material was not set, then generate a meterial for the mesh,
        // or use the default material if it failed to generate.
        if (Material == null) {
            Material = GenerateMaterial();
            if (Material == null) {
                Material = TerrainModelManager.Instance.DefaultMaterial;
            }
        }

        // Minimum base downsampling level should be 1.
        _baseDownsampleLevel = _baseDownsampleLevel < 1 ? 1 : _baseDownsampleLevel;

        // Add LOD group manager.
        // TODO Make this a class member variable.
        GameObject lodGroupContainer = new GameObject();
        lodGroupContainer.transform.SetParent(transform, false);
        lodGroupContainer.name = GameObjectName.LODGroupContainer;

        LODGroup lodGroup = lodGroupContainer.AddComponent<LODGroup>();
        LOD[] lods = new LOD[_lodLevels + 1];

        // Get Tiff file from the file path.

        // Create a child GameObject containing a mesh for each LOD level.
        for (int i = 0; i <= _lodLevels; i++) {

            GameObject child = new GameObject();
            child.transform.SetParent(lodGroupContainer.transform);

            // Name the LOD game object.
            child.name = $"LOD_{i}";

            // Use the parent's tranformations.
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.localEulerAngles = Vector3.zero;

            // Add MeshRenderer to child, and to the LOD group.
            MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
            lods[i] = new LOD(i == 0 ? 1 : Mathf.Pow(1 - (float)i / _lodLevels, 2), new Renderer[] { meshRenderer });

            // Add material to the MeshRenderer.
            if (Material != null) {
                meshRenderer.material = Material;
            }

            MeshFilter meshFilter = child.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();

            // Set the index format of the mesh to 32-bits, so that the mesh can have more than 65k vertices.
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            // Assign mesh data
            MeshData lodMeshData = meshData[i];

            float start = Time.realtimeSinceStartup;
            mesh.vertices = lodMeshData.Vertices;
            Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign vertices.");

            start = Time.realtimeSinceStartup;
            mesh.uv = lodMeshData.TexCoords;
            Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign UVs.");


            start = Time.realtimeSinceStartup;
            mesh.triangles = lodMeshData.Triangles;
            Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign triangles.");

            // This is a time consuming operation, and may cause the app to pause
            // for a couple of miliseconds since it runs on the main thread.
            start = Time.realtimeSinceStartup;
            mesh.RecalculateNormals();
            Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to recalculate normals.");

            meshFilter.mesh = mesh;

        }

        // Assign LOD meshes to LOD group.
        lodGroup.SetLODs(lods);

        // Calculate bounds if there are one or more LOD level. If there are no LOD levels, 
        // then we can just disable LOD, so there is no need to calculate bounds.
        if (_lodLevels > 0) {
            lodGroup.RecalculateBounds();
        }
        else {
            lodGroup.enabled = false;
        }

    }

    protected virtual Material GenerateMaterial() {

        Material material = new Material(Shader.Find("Standard"));
        material.SetColor("_Color", Color.white); // Set color to white so it doesn't tint the albedo.

        // TODO Also check if file exists.
        if (string.IsNullOrEmpty(_albedoFilePath)) {

            float start = Time.realtimeSinceStartup;

            RGBAImage srcImage;

            using (TiffWrapper tiff = new TiffWrapper(_albedoFilePath)) {
                srcImage = tiff.ToRGBAImage();
            }

            TextureCompressionFormat textureFormat = TextureCompressionFormat.DXT1;

            byte[] destBytes = TextureToolUtils.ImageToTexture(srcImage, textureFormat);
            Debug.Log($"Took {Time.realtimeSinceStartup - start} seconds to load texture.");

            start = Time.realtimeSinceStartup;
            Texture2D texture = new Texture2D(srcImage.Width, srcImage.Height, textureFormat.GetUnityFormat(), true);
            texture.GetRawTextureData<byte>().CopyFrom(destBytes);
            texture.Apply();
            Debug.Log($"Took {Time.realtimeSinceStartup - start} seconds to apply texture.");

            material.SetTexture("_MainTex", texture); // Set albedo texture.
        }

        return material;
    }

    protected abstract TerrainModelMetadata GenerateMetadata();

}
