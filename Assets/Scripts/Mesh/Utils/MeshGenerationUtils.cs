using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Common helper methods for generating meshes.
    /// </summary>
    public static class MeshGenerationUtils {

        /// <summary>
        ///     Generates the list of triangles for a grid topology mesh.
        /// </summary>
        public static int[] GenerateTriangles(int hVertCount, int vVertCount, bool reverse = false) {

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
                    result[startIndex + 1] = reverse ? rt : lb;
                    result[startIndex + 2] = rb;
                    result[startIndex + 3] = rb;
                    result[startIndex + 4] = reverse ? lb : rt;
                    result[startIndex + 5] = lt;

                    startIndex += 6;
                }
            }
            return result;
        }

        /// <summary>
        ///     Generates the list of triangles for a grid topology mesh.
        /// </summary>
        public static int[] GenerateTrianglesTransposed(int hVertCount, int vVertCount) {

            // The number of quads (triangle pairs) in each dimension is 
            // one less than the vertex counts in the respective dimensions.
            int hQuadCount = hVertCount - 1, vQuadCount = vVertCount - 1;

            int[] result = new int[6 * hQuadCount * vQuadCount];

            int startIndex = 0;
            for (int x = 0; x < hQuadCount; x++) {
                for (int y = 0; y < vQuadCount; y++) {

                    int lt = y + x * vVertCount;
                    int rt = lt + vVertCount;
                    int lb = lt + 1;
                    int rb = rt + 1;

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

        /// <summary>
        ///     Generates the UV coordinates of a vertex in a grid topology mesh.
        /// </summary>
        public static Vector2 GenerateUVCoord(int x, int y, int width, int height, Vector2 scale, Vector2 offset) {
            return new Vector2(scale.x * x / (width - 1) + offset.x, scale.y * y / (height - 1) + offset.y);
        }

        /// <summary>
        ///     Generates the UV coordinates of a vertex in a grid topology mesh.
        /// </summary>
        public static Vector2 GenerateUVCoord(int x, int y, int width, int height) {
            return new Vector2(x / (width - 1f), y / (height - 1f));
        }

    }
}
