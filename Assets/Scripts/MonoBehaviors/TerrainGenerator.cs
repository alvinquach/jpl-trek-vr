using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class TerrainGenerator : MonoBehaviour {

    [SerializeField] private string _filePath;

    [SerializeField] private SurfaceGeometryType _surfaceGeometryType = SurfaceGeometryType.Planar;

    [SerializeField] private float _scale = 1.0f;

    [SerializeField] private float _heightScale = 1.0f;

    [SerializeField] private int _downsample = 2;

    // Use this for initialization
    void Start() {

        if (_filePath == null) {
            return;
        }

        gameObject.AddComponent<MeshFilter>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        // GenerateMesh(mesh, _scale, _heightScale);
        DemToMeshUtils.GenerateMesh(_filePath, mesh, _surfaceGeometryType, _scale, _heightScale, _downsample);

    }

}
