using UnityEngine;

/// <summary>
///     Generates the mesh for a partial section of the planet defined
///     by bounding box coordinates. Does not use any height data;
///     as such, the resulting mesh will be a smooth surface.
/// </summary>
public class BasePartialTerrainMeshGenerator : TerrainMeshGenerator {

    // TEMPORARY
    private static readonly int LatLongVertCount = 50;

    protected float _radius;

    protected Vector4 _boundingBox;

    public BasePartialTerrainMeshGenerator(float radius, Vector4 boundingBox) {
        _radius = radius;
        _boundingBox = BoundingBoxUtils.ValidateBoundingBox(boundingBox);
    }

    public override void Generate() {

        float latStart = _boundingBox.y * Mathf.Deg2Rad;
        float latStop = _boundingBox.w * Mathf.Deg2Rad;
        float latSweep = latStop - latStart;

        float lonStart = _boundingBox.x;
        float lonStop = _boundingBox.z;
        float lonSweep = lonStop - lonStart;
        if (lonSweep > 180) {
            lonSweep = 360 - lonSweep;
            lonStart = _boundingBox.w;
            lonStop = lonStart + lonSweep;
        }

        Debug.Log($"{latSweep * Mathf.Rad2Deg}, {lonSweep}");

        float latIncrement = latSweep / (LatLongVertCount - 1);
        float lonIncrement = lonSweep / (LatLongVertCount - 1);

        Vector3[] verts = new Vector3[LatLongVertCount * LatLongVertCount];
        Vector2[] uvs = new Vector2[LatLongVertCount * LatLongVertCount];

        int yIndex = 0, vertexIndex = 0;
        for (float y = latStart; yIndex < LatLongVertCount; y += latIncrement) {

            int xIndex = 0;

            // Create a new vertex using the latitude angle. The coordinates of this
            // vertex will serve as a base for all the other vertices in this latitude.
            Vector3 baseLatVertex = new Vector3(Mathf.Cos(y), Mathf.Sin(y), 0);

            // Loop traverses backwards in order to get correct orientation of texture and normals.
            //for (float x = lonStop; x > lonStart; x -= lonIncrement) {
            for (float x = lonStart; xIndex < LatLongVertCount; x += lonIncrement) {

                // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                verts[vertexIndex] = Quaternion.Euler(0, -90 - x, 0) * (_radius * baseLatVertex);
                //Debug.Log($"({xIndex}, {yIndex}) -> ({xIndex / (LatLongVertCount - 1f)}, {-yIndex / (LatLongVertCount - 1f)})");
                uvs[vertexIndex] = GenerateStandardUV(xIndex, yIndex, LatLongVertCount, LatLongVertCount);
                xIndex++;
                vertexIndex++;
            }
            yIndex++;
        }

        InProgress = false;
        Complete = true;
        Progress = 1f;
        MeshData = new MeshData[] {
            new MeshData() {
                Vertices = verts,
                TexCoords = uvs,
                Triangles = GenerateTriangles(LatLongVertCount, LatLongVertCount)
            }
        };

    }

}