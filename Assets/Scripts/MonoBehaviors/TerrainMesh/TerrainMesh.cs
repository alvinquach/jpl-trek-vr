using UnityEngine;
using System;
using System.IO;
using System.Collections;
using BitMiracle.LibTiff.Classic;

public abstract class TerrainMesh : MonoBehaviour {

    public string DEMFilePath;

    public string AlbedoFilePath;

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
    protected int _LODLevels = 2;

    public int LodLevels {
        get { return _LODLevels; }
        set { if (!_init) _LODLevels = value; }
    }

    public Material Material;

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

        //if (DEMFile == null) {
        //    throw new Exception("Cannot create mesh. DEM file not definded.");
        //}

        // Minimum base downsampling level should be 1.
        _baseDownsampleLevel = _baseDownsampleLevel < 1 ? 1 : _baseDownsampleLevel;

        // Add LOD group manager.
        // TODO Make this a class member variable.
        LODGroup lodGroup = gameObject.AddComponent<LODGroup>();
        LOD[] lods = new LOD[_LODLevels + 1];

        // Get Tiff file from the file path.
        using (Tiff tiff = TiffUtils.FromFilePath(DEMFilePath)) {

            TiffUtils.PrintInfo(tiff, $"TIFF loaded from {DEMFilePath}");

            // Create a child GameObject containing a mesh for each LOD level.
            for (int i = 0; i <= _LODLevels; i++) {

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
                lods[i] = new LOD(i == 0 ? 1 : Mathf.Pow(1 - (float)i / _LODLevels, 2), new Renderer[] { meshRenderer });

                // Add material to the MeshRenderer.
                if (Material != null) {
                    meshRenderer.material = Material;
                }

                MeshFilter meshFilter = child.AddComponent<MeshFilter>();
                meshFilter.mesh = DemToMeshUtils.GenerateMesh(tiff, SurfaceGeometryType, _scale, _heightScale, _baseDownsampleLevel * (int)Mathf.Pow(2, i));

            }
        }

        // Assign LOD meshes to LOD group.
        lodGroup.SetLODs(lods);

        // Calculate bounds if there are one or more LOD level.
        // If there are no LOD levels, then we can just disable
        // LOD, so there is no need to calculate bounds.
        if (_LODLevels > 0) {
            lodGroup.RecalculateBounds();
        }
        else {
            lodGroup.enabled = false;
        }

        // Mark the TerrainMesh as already initialized.
        _init = true;
    }

}
