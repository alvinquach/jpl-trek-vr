using UnityEngine;

namespace TrekVRApplication {

    public sealed class TerrainMeshData : MeshData {

        //public int LatitudeVertexCount { get; set; }

        //public int LongitudeVertexCount { get; set; }

        public Vector3[] ExtraVertices { get; set; }

        public int[] ExtraTriangles { get; set; }

        public Vector3 MinimumVertex { get; set; }

        public Vector3 MaximumVertex { get; set; }

    }

}