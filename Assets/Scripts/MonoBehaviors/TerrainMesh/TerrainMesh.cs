using UnityEngine;
using System;
using System.IO;
using System.Collections;
using BitMiracle.LibTiff.Classic;

public abstract class TerrainMesh : MonoBehaviour {

    [SerializeField]
    protected string _demFilePath;

    public string DemFilePath {
        get { return _demFilePath; }
        set { if (!_init) _demFilePath = value; }
    }

    [SerializeField]
    protected string _albedoFilePath;

    public string AlbedoFilePath {
        get { return _albedoFilePath; }
        set { if (!_init) _albedoFilePath = value; }
    }

    // TODO Split this into 'radius' and 'size' for spherical 
    // and planar meshes, respectively.
    [SerializeField]
    protected float _scale;

    public float Scale {
        get { return _scale; }
        set { if (!_init) _scale = value; }
    }   

    [SerializeField]
    protected float _heightScale = 1.0f;

    public float HeightScale {
        get { return _heightScale; }
        set { if (!_init) _heightScale = value; }
    }

    [SerializeField]
    protected int _baseDownsampleLevel = 2;

    public int BaseDownSampleLevel {
        get { return _baseDownsampleLevel; }
        set { if (!_init) _baseDownsampleLevel = value; }
    }

    [SerializeField]
    protected int _lodLevels = 2;

    public int LodLevels {
        get { return _lodLevels; }
        set { if (!_init) _lodLevels = value; }
    }

    public Material Material { get; set; }

    // TODO Add option to use linear LOD downsampling.

    public abstract TerrainGeometryType SurfaceGeometryType { get; }

    protected bool _init = false;

    // Use this for initialization
    void Start() {
        InitMesh();
    }

    // Can only be called once.
    public virtual void InitMesh() {

        // TerrrainMesh can only be initialized once.
        if (_init) {
            return;
        }

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
        using (Tiff tiff = TiffUtils.FromFilePath(_demFilePath)) {

            TiffUtils.PrintInfo(tiff, $"TIFF loaded from {_demFilePath}");

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
                meshFilter.mesh = GenerateMesh(tiff);
                //meshFilter.mesh = DemToMeshUtils.GenerateMesh(tiff, SurfaceGeometryType, _scale, _heightScale, _baseDownsampleLevel * (int)Mathf.Pow(2, i));

            }
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

        // Mark the TerrainMesh as already initialized.
        _init = true;
    }

    /// <summary>
    ///     The implementing subclass should make a call to the correct method
    ///     in DemToMeshUtils for generating the corresponding mesh type.
    /// </summary>
    protected abstract Mesh GenerateMesh(Tiff tiff);

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
