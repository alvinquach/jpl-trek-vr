using UnityEngine;

namespace TrekVRApplication {

    // Should this be a class or a struct?
    public class MeshData {
        public Vector3[] Vertices { get; set; }
        public Vector2[] TexCoords { get; set; }
        public int[] Triangles { get; set; }

    }

}
