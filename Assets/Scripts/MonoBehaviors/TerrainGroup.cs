using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class TerrainGroup : MonoBehaviour {

    [SerializeField] private string _filePath;

    [SerializeField] private SurfaceGeometryType _surfaceGeometryType = SurfaceGeometryType.Planar;

    [SerializeField] private float _scale = 1.0f;

    [SerializeField] private float _heightScale = 1.0f;

    [SerializeField] private int _baseDownsampleLevel = 2;

    [SerializeField] private int _LODLevels = 2;

    [SerializeField] private Material _material;

    // TODO Add option to use linear LOD downsampling.

    // Use this for initialization
    void Start() {

        if (_filePath == null) {
            return;
        }

        // Minimum base downsampling level should be 1.
        _baseDownsampleLevel = _baseDownsampleLevel < 1 ? 1 : _baseDownsampleLevel;

        // Add LOD group manager.
        // TODO Make this a class member variable.
        LODGroup lodGroup = gameObject.AddComponent<LODGroup>();
        LOD[] lods = new LOD[_LODLevels + 1];

        // Create a child GameObject containing a mesh for each LOD level.
        for (int i = 0; i <= _LODLevels; i++) {
            GameObject child = new GameObject();
            child.transform.SetParent(transform);
            child.name = "LOD_" + i;

            // Add MeshRenderer to child, and to the LOD group.
            MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
            lods[i] = new LOD(i == 0 ? 1 : Mathf.Pow(1 - (float)i / _LODLevels, 2), new Renderer[] { meshRenderer });

            // Add material to the MeshRenderer.
            if (_material != null) {
                meshRenderer.material = _material;
            }

            Mesh mesh = child.AddComponent<MeshFilter>().mesh;
            DemToMeshUtils.GenerateMesh(_filePath, mesh, _surfaceGeometryType, _scale, _heightScale, _baseDownsampleLevel * (int)Mathf.Pow(2, i));

        }

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

    }

}
