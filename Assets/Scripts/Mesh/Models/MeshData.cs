using UnityEngine;

namespace TrekVRApplication {

    public class MeshData {

        public Vector3[] Vertices { get; set; }

        public Vector2[] TexCoords { get; set; }

        public int[] Triangles { get; set; }

        // TODO Add normals

        public static bool operator true(MeshData o) {
            return o != null;
        }

        public static bool operator false(MeshData o) {
            return o == null;
        }

        public static bool operator !(MeshData o) {
            return o ? false : true;
        }

    }

}
