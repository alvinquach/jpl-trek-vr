using UnityEngine;
using System.Collections;

namespace TrekVRApplication {

    /// <summary>
    ///     Common helper methods for generating localized terrain meshes.
    /// </summary>
    public static class LocalTerrainMeshGenerationUtils {

        public static Vector2 GenerateUVCoord(int x, int y, int lonVertCount, int latVertCount, UVBounds uvBounds) {
            Vector2 uvScale = new Vector2(uvBounds.U2 - uvBounds.U1, uvBounds.V2 - uvBounds.V1);
            Vector2 uvOffset = new Vector2(-uvBounds.U1, -uvBounds.V1);
            return MeshGenerationUtils.GenerateUVCoord(x, latVertCount - y - 1, lonVertCount, latVertCount, uvScale, uvOffset);
        }

        /// <summary>
        ///     Generates a new vertex using the latitude angle. The coordinates
        ///     of the genereated vertex will serve as a base for all the other 
        ///     vertices of the same latitude.
        /// </summary>
        /// <param name="latitude">Angle in degrees.</param>
        /// <returns></returns>
        public static Vector3 GenerateBaseLatitudeVertex(float latitude) {
            latitude *= Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(latitude), Mathf.Sin(latitude), 0);
        }

        /// <summary>
        ///     Generates new vertex coordinates based on an existing coordinate
        ///     adjusted for longitude and offset from bounding box.
        /// </summary>
        /// <param name="baseVertex">
        ///     The existing vertex coordinates with "height" and latitude (but
        ///     not longitude) already accounted for.
        /// </param>
        /// <param name="longitude">Angle in degrees.</param>
        /// <param name="latLongOffset">
        ///     The coordinate offset that needs to be applied so that the overall
        ///     mesh faces the (0°, 0°) direction.
        /// </param>
        /// <returns></returns>
        public static Vector3 GenerateVertex(Vector3 baseVertex, float longitude, Vector2 latLongOffset, float radius) {

            // Apply y-axis rotation first to correct for longitude offset.
            Vector3 result = Quaternion.Euler(0, latLongOffset.y - longitude, 0) * baseVertex;

            // Then, apply z-axis rotation to correct for latitude offset.
            result = Quaternion.Euler(0, 0, -latLongOffset.x) * result;

            // Apply radius offset.
            result -= new Vector3(radius, 0, 0);

            return result;
        }

        public static void ProcessEdgeVertices(Vector3[] edgeVertices, float offset) {
            int lastIndex = edgeVertices.Length / 2 - 1;

            // Repeat the first vertex to form a loop.
            edgeVertices[lastIndex] = edgeVertices[0];

            // Copy the other vertices minus the x-values.
            for (int i = 0; i <= lastIndex; i++) {
                Vector3 vertex = edgeVertices[i];
                edgeVertices[i + edgeVertices.Length / 2] = new Vector3(offset, vertex.y, vertex.z);
            }
        }

    }

}
