using System;
using UnityEngine;

namespace TrekVRApplication {

    public class GenerateCylindricalMenuMeshTask : GenerateMenuMeshTask {

        private float _angleSweep;
        private float _height;
        private float _radius;
        private int _sides = 32;
        private int _heightSegments = 1;
        private RelativePosition _alignment;

        public GenerateCylindricalMenuMeshTask(float angleSweep, float height, float radius,
            RelativePosition alignment = RelativePosition.Bottom) {

            _angleSweep = angleSweep * Mathf.Deg2Rad;
            _height = height;
            _radius = radius;
            _alignment = alignment;
        }

        public GenerateCylindricalMenuMeshTask(float angleSweep, float height, float radius, int sides,
            RelativePosition alignment = RelativePosition.Bottom) : this(angleSweep, height, radius, alignment) {

            if (sides < 2) {
                throw new Exception("The number of sides must be at least 2.");
            }
            _sides = sides;
        }

        public GenerateCylindricalMenuMeshTask(float angleSweep, float height, float radius, int sides, int heightSegments,
            RelativePosition alignment = RelativePosition.Bottom) : this(angleSweep, height, radius, sides, alignment) {

            if (sides < 1) {
                throw new Exception("The number of vertical segments must be at least 1.");
            }
            _heightSegments = heightSegments;
        }

        protected override void Generate() {

            int hVertCount = _sides + 1;
            int vVertCount = _heightSegments + 1;

            float startAngle = GetStartAngle();
            float angleStep = _angleSweep / _sides;

            float startHeight = GetStartHeight();
            float heightStep = _height / _heightSegments;

            Vector3[] verts = new Vector3[hVertCount * vVertCount];
            Vector2[] uvs = new Vector2[hVertCount * vVertCount];
            int vertexIndex = 0;

            for (int x = 0; x < hVertCount; x++) {
                float angle = angleStep * x + startAngle;
                Vector3 baseVertex = _radius * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                for (int y = 0; y < vVertCount; y++) {
                    Vector3 vertex = baseVertex + new Vector3(0, startHeight - heightStep * y, 0);
                    verts[vertexIndex] = vertex;
                    uvs[vertexIndex] = MeshGenerationUtils.GenerateUVCoord(x, y, hVertCount, vVertCount, -Vector2.one, Vector2.one);
                    vertexIndex++;
                }
            }

            _progress = 1f;

            // Only one LOD is generated.
            _meshData = new MeshData[] {
                new MeshData() {
                    Vertices = verts,
                    TexCoords = uvs,
                    Triangles = MeshGenerationUtils.GenerateTrianglesTransposed(hVertCount, vVertCount)
                }
            };
        }

        private float GetStartAngle() {
            switch (_alignment) {
                case RelativePosition.TopLeft:
                case RelativePosition.Left:
                case RelativePosition.BottomLeft:
                    return _angleSweep / 2;
                case RelativePosition.TopRight:
                case RelativePosition.Right:
                case RelativePosition.BottomRight:
                    return -_angleSweep / 2;
                default:
                    return 0;
            }
        }

        private float GetStartHeight() {
            switch (_alignment) {
                case RelativePosition.TopLeft:
                case RelativePosition.Top:
                case RelativePosition.TopRight:
                    return 0;
                case RelativePosition.BottomLeft:
                case RelativePosition.Bottom:
                case RelativePosition.BottomRight:
                    return _height;
                default:
                    return _height / 2;
            }
        }

    }

}
