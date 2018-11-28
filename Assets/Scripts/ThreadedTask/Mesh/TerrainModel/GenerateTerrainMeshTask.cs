using UnityEngine;

namespace TrekVRApplication {

    public abstract class GenerateTerrainMeshTask : ThreadedTask<float, MeshData[]> {

        protected TerrainModelMetadata _metadata;

        protected MeshData[] _meshData;

        protected float _progress = 0.0f;

        public GenerateTerrainMeshTask(TerrainModelMetadata metadata) {
            _metadata = metadata;
            // TODO Check if baseDownsample is power of 2.
        }

        public override float GetProgress() {
            return _progress;
        }

        protected sealed override MeshData[] Task() {
            Generate();
            return _meshData;
        }

        /// <summary>Generate the mesh data and store it in the member variable.</summary>
        protected abstract void Generate();

        /// <summary>Generates the list of triangles.</summary>
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

        /// <summary>Generates the list of UV coordinates.</summary>
        protected Vector2 GenerateStandardUV(int x, int y, int width, int height) {
            return new Vector2(x / (width - 1f), y / (height - 1f));
        }

    }

}
