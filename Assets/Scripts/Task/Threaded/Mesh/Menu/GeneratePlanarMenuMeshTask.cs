using System;
using UnityEngine;

namespace TrekVRApplication {

    public class GeneratePlanarMenuMeshTask : GenerateMenuMeshTask {

        private float _width;
        private float _height;
        private int _widthSegments = 2;
        private int _heightSegments = 2;
        private RelativePosition _alignment;

        public GeneratePlanarMenuMeshTask(float width, float height,
            RelativePosition alignment = RelativePosition.TopLeft) {

            _width = width;
            _height = height;
            _alignment = alignment;
        }

        public GeneratePlanarMenuMeshTask(float width, float height, int widthSegments, int heightSegments,
            RelativePosition alignment = RelativePosition.Bottom) : this(width, height, alignment) {

            _widthSegments = widthSegments;
            _heightSegments = heightSegments;
        }

        protected override void Generate() {

            int hVertCount = _widthSegments + 1;
            int vVertCount = _heightSegments + 1;

            float startWidth = GetStartX();
            float widthStep = _width / _widthSegments;

            float startHeight = GetStartY();
            float heightStep = _height / _heightSegments;

            Vector3[] verts = new Vector3[hVertCount * vVertCount];
            Vector2[] uvs = new Vector2[hVertCount * vVertCount];
            int vertexIndex = 0;

            for (int x = 0; x < hVertCount; x++) {
                Vector3 baseVertex =  new Vector3(widthStep * x + startWidth, 0, 0);

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

        private float GetStartX() {
            switch (_alignment) {
                case RelativePosition.TopLeft:
                case RelativePosition.Left:
                case RelativePosition.BottomLeft:
                    return -_width;
                case RelativePosition.TopRight:
                case RelativePosition.Right:
                case RelativePosition.BottomRight:
                    return 0;
                default:
                    return -_width / 2;
            }
        }

        private float GetStartY() {
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
