using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class TerrainGenerator : MonoBehaviour {

    [SerializeField] private string _filePath;

    [SerializeField] private float _scale = 1.0f;

    [SerializeField] private float _heightScale = 1.0f;

    [SerializeField] private bool _downsample = false;

    // Use this for initialization
    void Start() {

        if (_filePath == null) {
            return;
        }

        gameObject.AddComponent<MeshFilter>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        GenerateMesh(mesh, _scale, _heightScale);
    }

    // Update is called once per frame
    void Update() {

    }

    void GenerateMesh(Mesh mesh, float scale, float heightScale) {

        if (String.IsNullOrEmpty(_filePath)) {
            Debug.LogError("No TIFF file specified for " + this.GetType().Name);
            return;
        }

        using (Tiff tiff = Tiff.Open(_filePath, "r")) {

            if (tiff == null) {
                Debug.LogError("Cannot open TIFF from " + _filePath);
                return;
            }

            FieldValue[] res = tiff.GetField(TiffTag.IMAGELENGTH);
            int height = res[0].ToInt();

            res = tiff.GetField(TiffTag.IMAGEWIDTH);
            int width = res[0].ToInt();

            res = tiff.GetField(TiffTag.BITSPERSAMPLE);
            short bpp = res[0].ToShort();

            res = tiff.GetField(TiffTag.SAMPLESPERPIXEL);
            short spp = res[0].ToShort();

            // Currently, only 16-bit and 32-bit grayscale files are supported.
            if ((bpp != 32 && bpp != 16) || spp != 1) {
                return;
            }

            Debug.Log(width + "x" + height + "@" + bpp);

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.Clear();

            // Vertex counts in the horizontal and vertical directions are
            // the same as texture width and height, respectively.
            int hVertCount = width / (_downsample ? 2 : 1), vVertCount = height / (_downsample ? 2 : 1);

            byte[] scanline = new byte[tiff.ScanlineSize()];

            Vector3[] vertices = new Vector3[hVertCount * vVertCount];
            Vector2[] uvs = new Vector2[hVertCount * vVertCount];

            int vertexIndex = 0;
            float min = float.MaxValue;
            for (int y = 0; y < vVertCount; y++) {
                tiff.ReadScanline(scanline, y * (_downsample ? 2 : 1));
                float[] values = bpp == 32 ? Scanline32ToFloat(scanline) : Scanline16ToFloat(scanline);
                for (int x = 0; x < hVertCount; x++) {
                    float value = values[x * (_downsample ? 2 : 1)];
                    vertices[vertexIndex] = new Vector3(x * scale, value * heightScale, y * scale);
                    uvs[vertexIndex] = new Vector2(x / (float)hVertCount, -y / (float)vVertCount);
                    min = value < min ? value : min;
                    vertexIndex++;
                }
            }

            transform.position -= min * heightScale * Vector3.up;

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = GenerateTriangles(hVertCount, vVertCount);


            mesh.RecalculateNormals();
            Debug.Log(mesh.normals.Length);

        }
    }

    private float[] Scanline32ToFloat(byte[] scanline) {
        if (scanline.Length % 4 != 0) {
            return null;
        }
        int length = scanline.Length / 4;
        float[] result = new float[length];
        for (int i = 0; i < length; i++) {
            // TODO Check if CPU uses little endian.
            float value = BitConverter.ToSingle(scanline, 4 * i);
            result[i] = value;
        }
        return result;
    }

    private float[] Scanline16ToFloat(byte[] scanline) {
        if (scanline.Length % 2 != 0) {
            return null;
        }
        int length = scanline.Length / 2;
        float[] result = new float[length];
        for (int i = 0; i < length; i++) {
            // TODO Check if CPU uses little endian.
            float value = BitConverter.ToUInt16(scanline, 2 * i);
            result[i] = value / ushort.MaxValue;
        }
        return result;
    }

    private int[] GenerateTriangles(int hVertCount, int vVertCount) {

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

}
