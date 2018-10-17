using System.Threading;
using UnityEngine;

public abstract class TerrainMeshGenerator {

    protected string _filepath;

    protected float _heightScale;

    protected int _lodLevels;

    public MeshData[] MeshData { get; protected set; }

    public bool InProgress { get; protected set; } = false;

    public bool Complete { get; protected set; } = false;

    public float Progress { get; protected set; } = 0f;

    public TerrainMeshGenerator(string filepath, float heightScale, int lodLevels) {
        _filepath = filepath;
        _heightScale = heightScale;
        _lodLevels = lodLevels;
    }

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.
    // TODO Maybe support other image types?

    public abstract void Generate();

    public void GenerateAsync() {

        Thread thread = new Thread(
            new ThreadStart(Generate)
        );

        thread.Start();
    }

    protected Vector2 GenerateStandardUV(int x, int y, int width, int height) {
        return new Vector2(x / (width - 1f), -y / (height - 1f));
    }

}
