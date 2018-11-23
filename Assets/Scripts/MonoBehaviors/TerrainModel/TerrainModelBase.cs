using UnityEngine;
using System;
using System.IO;
using Nvidia.TextureTools;
using System.Runtime.InteropServices;
using Unity.Collections;

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
            MeshData meshData = meshGenerator.MeshData[i];

            float start = Time.realtimeSinceStartup;
            mesh.vertices = meshData.Vertices;
            Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign vertices.");

            start = Time.realtimeSinceStartup;
            mesh.uv = meshData.TexCoords;
            Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign UVs.");


            start = Time.realtimeSinceStartup;
            mesh.triangles = meshData.Triangles;
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

        // TODO Also check if file exists.
        if (!String.IsNullOrEmpty(_albedoFilePath)) {

            float start = Time.realtimeSinceStartup;

            RGBAImage srcImage;

            using (TiffWrapper tiff = new TiffWrapper(_albedoFilePath)) {
                srcImage = tiff.ToRGBAImage();
            }

            byte[] srcBytes = srcImage.ToByteArray();

            texture = new Texture2D(srcImage.Width, srcImage.Height, TextureFormat.DXT1, true);
            NativeArray<byte> rawTextureData = texture.GetRawTextureData<byte>();
            byte[] destBytes = new byte[rawTextureData.Length];

            GCHandle pinnedArray = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            Debug.Log("Hello 0");

            InputOptions inputOptions = new InputOptions();
            inputOptions.SetTextureLayout(TextureType.Texture2D, srcImage.Width, srcImage.Height, 1);
            inputOptions.SetMipmapData(pointer, srcImage.Width, srcImage.Height, 1, 0, 0);
            inputOptions.SetMipmapGeneration(true);
            inputOptions.SetMipmapFilter(MipmapFilter.Box);
            inputOptions.SetAlphaMode(AlphaMode.None);

            CompressionOptions compressionOptions = new CompressionOptions();
            compressionOptions.SetFormat(Format.BC1);

            OutputOptions outputOptions = new OutputOptions();

            int mipIndex = -1;
            int destIndex = 0;
            outputOptions.SetOutputOptionsOutputHandler(
                (size, width, height, depth, face, miplevel) => {
                //Debug.Log($"Mip level: {miplevel}/nSize: {size}, Width: {width}, Height: {height}");
                mipIndex = miplevel;
                },
                (data, size) => {
                    if (mipIndex >= 0) {
                        Marshal.Copy(data, destBytes, destIndex, size);
                        destIndex += size;
                    }
                    return true;
                },
                () => { }
            );

            Compressor compressor = new Compressor();
            compressor.Compress(inputOptions, compressionOptions, outputOptions);


            pinnedArray.Free();
            Debug.Log($"Took {Time.realtimeSinceStartup - start} seconds to load texture.");

            start = Time.realtimeSinceStartup;
            rawTextureData.CopyFrom(destBytes);
            texture.Apply();
            Debug.Log($"Took {Time.realtimeSinceStartup - start} seconds to apply texture.");
        }

        // TODO This is temporary
        else {
            TiffTexture2DConverter textureConverter = new TiffTexture2DConverter(null, 1024, 1024);
            textureConverter.Convert();

            texture = new Texture2D(1024, 1024);
            texture.SetPixels32(textureConverter.Pixels);
            texture.Apply();
            texture.Compress(true);
        }

        if (texture == null) {
            return null; // TODO Throw exception
        }

        Material material = new Material(Shader.Find("Standard"));
        material.SetColor("_Color", Color.white); // Set color to white so it doesn't tint the albedo.
        material.SetTexture("_MainTex", texture); // Set albedo texture.
        return material;
    }

}
