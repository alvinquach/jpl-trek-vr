using System.Threading;
using UnityEngine;

public abstract class TerrainMeshGenerator {

    public MeshData[] MeshData { get; protected set; }

    public bool InProgress { get; protected set; } = false;

    public bool Complete { get; protected set; } = false;

    public float Progress { get; protected set; } = 0f;

    public abstract void Generate();

    public void GenerateAsync() {

        Thread thread = new Thread(
            new ThreadStart(Generate)
        );

        thread.Start();
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

    public static bool operator true(TerrainMeshGenerator o) {
        return o != null;
    }

    public static bool operator false(TerrainMeshGenerator o) {
        return o == null;
    }

    public static bool operator !(TerrainMeshGenerator o) {
        return o ? false : true;
    }

}
