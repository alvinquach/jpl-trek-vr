using UnityEngine;

namespace TrekVRApplication {

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TerrainOverlayArea : TerrainOverlayObject {

        private Mesh _mesh;

        public override Material Material {
            get {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                return meshRenderer.material;
            }
            set {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = value;
            }
        }

        protected override void Awake() {
            base.Awake();

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = _mesh = GenerateMesh();
        }

        public void UpdateArea(IBoundingBox bbox) {
            IBoundingBox currentBoundingBox = Controller.CurrentBoundingBox;
            if (currentBoundingBox == BoundingBox.Zero) {
                return;
            }
            UVBounds uvBounds = BoundingBoxUtils.CalculateUVBounds(currentBoundingBox, bbox);
            UpdateArea(uvBounds);
        }

        public void UpdateArea(UVBounds uvBounds) {

            float horizontalScale = Controller.RenderTextureAspectRatio;
            bool reverseOrder = (uvBounds.U1 < uvBounds.U2) ^ (uvBounds.V1 < uvBounds.V2);

            Vector3[] verts;

            if (reverseOrder) {
                verts = new Vector3[] {
                    new Vector3(horizontalScale * uvBounds.U2, uvBounds.V1),
                    new Vector3(horizontalScale * uvBounds.U2, uvBounds.V2),
                    new Vector3(horizontalScale * uvBounds.U1, uvBounds.V1),
                    new Vector3(horizontalScale * uvBounds.U1, uvBounds.V2)
                };
            }
            else {
                verts = new Vector3[] {
                    new Vector3(horizontalScale * uvBounds.U1, uvBounds.V1),
                    new Vector3(horizontalScale * uvBounds.U1, uvBounds.V2),
                    new Vector3(horizontalScale * uvBounds.U2, uvBounds.V1),
                    new Vector3(horizontalScale * uvBounds.U2, uvBounds.V2)
                };
            }

            _mesh.vertices = verts;

            if (gameObject.activeInHierarchy) {
                Controller.UpdateTexture();
            }
        }

        private Mesh GenerateMesh() {

            Vector3[] verts = new Vector3[] {
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 0),
                new Vector3(1, 1)
            };

            Vector2[] uvs = new Vector2[] {
                new Vector3(0, 0),
                new Vector3(0, 1),
                new Vector3(1, 0),
                new Vector3(1, 1)
            };

            int[] tris = MeshGenerationUtils.GenerateTrianglesTransposed(2, 2);

            return new Mesh {
                vertices = verts,
                uv = uvs,
                triangles = tris
            };
        }

    }

}
