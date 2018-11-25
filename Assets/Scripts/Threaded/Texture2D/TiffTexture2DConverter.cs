using System.Threading;
using UnityEngine;
using UnityEditor;

public class TiffTexture2DConverter {

    private string _filepath;
    private int _width;
    private int _height;

    public Color32[] Pixels { get; private set; }

    public bool InProgress { get; private set; } = false;

    public bool Complete { get; private set; } = false;

    public float Progress { get; private set; } = 0f;

    public TiffTexture2DConverter(string filepath, int width, int height) {
        _filepath = filepath;
        _width = width;
        _height = height;
    }

    public void Convert() {
        Color32[] pixels = new Color32[_width * _height];

        int pixelIndex = 0;

        for (int y = 0; y < _height; y++) {
            for (int x = 0; x < _width; x++) {
                pixels[pixelIndex++] = new Color32(
                    (byte)(x % 255),
                    (byte)((x + y) % 255),
                    (byte)(y % 255),
                    255
                );
            }
        }

        InProgress = false;
        Complete = true;
        Progress = 1f;
        Pixels = pixels;
    }

    public void ConvertAsync() {

        Thread thread = new Thread(
            new ThreadStart(Convert)
        );

        thread.Start();
    }

}