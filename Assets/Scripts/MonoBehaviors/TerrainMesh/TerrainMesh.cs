using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BitMiracle.LibTiff.Classic;

public abstract class TerrainMesh : MonoBehaviour {

    [SerializeField]
    protected string _demFilePath;

    public string DemFilePath {
        get { return _demFilePath; }
        set { if (!_initStarted) _demFilePath = value; }
    }

    [SerializeField]
    protected string _albedoFilePath;

    public string AlbedoFilePath {
        get { return _albedoFilePath; }
        set { if (!_initStarted) _albedoFilePath = value; }
    }

    [SerializeField]
    protected float _heightScale = 1.0f;

    public float HeightScale {
        get { return _heightScale; }
        set { if (!_initStarted) _heightScale = value; }
    }

    [SerializeField]
    protected int _baseDownsampleLevel = 2;

    public int BaseDownSampleLevel {
        get { return _baseDownsampleLevel; }
        set { if (!_initStarted) _baseDownsampleLevel = value; }
    }

    // TODO Add option to use linear LOD downsampling.

    [SerializeField]
    protected int _lodLevels = 2;

    public int LodLevels {
        get { return _lodLevels; }
        set { if (!_initStarted) _lodLevels = value; }
    }

    public Material Material { get; set; }

    protected bool _initStarted = false;
    protected bool _initCompleted = false;

    protected abstract TiffTerrainMeshGenerator MeshGenerator { get; }

    // Use this for initialization
    void Start() {
        GenerateMeshData();
    }

    void Update() {
        if (!_initCompleted && MeshGenerator.Complete) {
            ProcessMeshData();
            _initCompleted = true;
        }
    }

    // Can only be called once.
    public virtual void GenerateMeshData() {
        if (_initStarted) {
            return;
        }

        MeshGenerator.GenerateAsync();
        _initStarted = true;
    }

    protected virtual void ProcessMeshData() {

        // If material was not set, then generate a meterial for the mesh,
        // or use the default material if it failed to generate.
        if (Material == null) {
            Material = GenerateMaterial();
            if (Material == null) {
                Material = TerrainMeshController.Instance.DefaultMaterial;
            }
        }

        // Minimum base downsampling level should be 1.
        _baseDownsampleLevel = _baseDownsampleLevel < 1 ? 1 : _baseDownsampleLevel;

        // Add LOD group manager.
        // TODO Make this a class member variable.
        LODGroup lodGroup = gameObject.AddComponent<LODGroup>();
        LOD[] lods = new LOD[_lodLevels + 1];

        // Get Tiff file from the file path.

        // Create a child GameObject containing a mesh for each LOD level.
        for (int i = 0; i <= _lodLevels; i++) {

            GameObject child = new GameObject();
            child.transform.SetParent(transform);

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
            MeshData meshData = MeshGenerator.MeshData[i];
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
        Material defaultMaterial = TerrainMeshController.Instance.DefaultMaterial;
        Texture2D texture = TextureUtils.GenereateTextureFromTiff(_albedoFilePath);
        if (texture == null || defaultMaterial == null) {
            return null; // TODO Throw exception
        }
        Material material = new Material(defaultMaterial);
        material.SetTexture("_MainTex", texture);
        return material;
    }

}
