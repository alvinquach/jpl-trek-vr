using BitMiracle.LibTiff.Classic;
using System;
using System.Threading;
using UnityEngine;

public abstract class TiffTerrainMeshGenerator {

    protected string _filepath;

    public MeshData[] MeshData { get; protected set; }

    public bool InProgress { get; protected set; } = false;

    public bool Complete { get; protected set; } = false;

    public float Progress { get; protected set; } = 0f;

    public TiffTerrainMeshGenerator(string filepath) {
        _filepath = filepath;
    }

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.
    // TODO Maybe support other image types?

    public abstract MeshData[] Generate(float scale, float heightScale, int lodLevels, int baseDownsample);

    public void GenerateAsync(float scale, float heightScale, int lodLevels, int baseDownsample) {

        Thread meshGenerationThread = new Thread(
            new ThreadStart(() => {
                Generate(scale, heightScale, lodLevels, baseDownsample);
            })
        );

        meshGenerationThread.Start();
    }

    /// <summary>
    ///     Checks of the Tiff image can be used as a height map for generating meshes.
    ///     For use as a height map, the image must be in 16-bit or 32-bit grayscale.
    ///     Both scanline and tiled encoding are support.
    ///     Throws an error if an invalid or incompatible Tiff image is used.
    /// </summary>
    protected void ValidateTiff(TiffInfo info) {

        if (info == null) {
            throw new Exception("TIFF file cannot be null."); // TODO Create exception for this.
        }

        // Currently, only 16-bit and 32-bit grayscale files are supported.
        if (info.BPP != 32 && info.BPP != 16 || info.SPP != 1) {
            throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
        }

    }

    private Mesh CreateNewMesh() {
        Mesh mesh = new Mesh();

        // Set the index format of the mesh to 32-bits, so that the mesh can have more than 65k vertices.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        return mesh;
    }

    protected int[] GenerateTriangles(int hVertCount, int vVertCount) {

        // The number of quads (triangle pairs) in each dimension is 
        // one less than the vertex counts in the respective dimensions.
        int hQuadCount = hVertCount - 1, vQuadCount = vVertCount - 1;

        int[] result = new int[6 * hQuadCount * vQuadCount];

        int startIndex = 0;
        for (int y = 0; y < vQuadCount; y++) {
            for (int x = 0; x < hQuadCount; x++) {

                int lt = x + y * hVertCount;
                int rt = lt + 1;
                int lb = lt + hVertCount;
                int rb = lb + 1;

                // TODO Alternate the triangle orientation of each consecutive quad.

                result[startIndex] = lt;
                result[startIndex + 1] = lb;
                result[startIndex + 2] = rb;
                result[startIndex + 3] = rb;
                result[startIndex + 4] = rt;
                result[startIndex + 5] = lt;

                startIndex += 6;
            }
        }
        return result;
    }

    protected Vector2 GenerateStandardUV(int x, int y, int width, int height) {
        return new Vector2(x / (width - 1f), -y / (height - 1f));
    }

}
