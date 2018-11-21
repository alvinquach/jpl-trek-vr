﻿using UnityEngine;
using System;
using System.IO;

public abstract class TerrainModelBase : MonoBehaviour {

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
    ///     This should be implemented in a way such that a TerrainMeshGenerator
    ///     object is always available (not null) while _initTaskStatus is Started
    ///     or completed, and always null if _initTaskStatus is NotStarted.
    /// </summary>
    protected abstract TerrainMeshGenerator MeshGenerator { get; }

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

    protected virtual void Start() {
        GenerateMeshData();
    }

    protected virtual void Update() {
        if (_initTaskStatus == TaskStatus.Started) {
            if (MeshGenerator.Complete) {
                ProcessMeshData();
                _initTaskStatus = TaskStatus.Completed;
            }
        }
    }

    // Can only be called once.
    public virtual void GenerateMeshData() {
        if (_initTaskStatus > TaskStatus.NotStarted) {
            return;
        }
        _initTaskStatus = TaskStatus.Started;
        MeshGenerator.GenerateAsync();
    }

    protected virtual void ProcessMeshData(TerrainMeshGenerator meshGenerator = null) {

        if (meshGenerator == null) {
            meshGenerator = MeshGenerator;
        }

        // If material was not set, then generate a meterial for the mesh,
        // or use the default material if it failed to generate.
        if (Material == null) {
            Material = GenerateMaterial();
            if (Material == null) {
                Material = TerrainModelService.Instance.DefaultMaterial;
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
            child.name = "LOD_" + i;

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
            MeshData meshData = meshGenerator.MeshData[i];
            mesh.vertices = meshData.Vertices;
            mesh.uv = meshData.TexCoords;
            mesh.triangles = meshData.Triangles;

            // This is a time consuming operation, and may cause the app to pause
            // for a couple of miliseconds since it runs on the main thread.
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;

        }

        // Assign LOD meshes to LOD group.
        lodGroup.SetLODs(lods);

        // Calculate bounds if there are one or more LOD level.
        // If there are no LOD levels, then we can just disable
        // LOD, so there is no need to calculate bounds.
        if (_lodLevels > 0) {
            lodGroup.RecalculateBounds();
        }
        else {
            lodGroup.enabled = false;
        }

    }

    protected virtual Material GenerateMaterial() {


        Texture2D texture;
        if (!String.IsNullOrEmpty(_albedoFilePath)) {
            texture = new Texture2D(1024, 1024, TextureFormat.DXT5, true);
            byte[] imageData = File.ReadAllBytes(_albedoFilePath);
            texture.LoadImage(imageData);
        }
        else {
            // TODO This is temporary
            TiffTexture2DConverter textureConverter = new TiffTexture2DConverter(null, 1024, 1024);
            textureConverter.Convert();

            texture = new Texture2D(1024, 1024);
            texture.SetPixels32(textureConverter.Pixels);
            texture.Apply();
            texture.Compress(true);
        }
        Debug.Log(texture.format);

        if (texture == null) {
            return null; // TODO Throw exception
        }

        Material material = new Material(Shader.Find("Standard"));
        material.SetColor("_Color", Color.white); // Set color to white so it doesn't tint the albedo.
        material.SetTexture("_MainTex", texture); // Set albedo texture.
        return material;
    }

}