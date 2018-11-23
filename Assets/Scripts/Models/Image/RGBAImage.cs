using UnityEngine;

public class RGBAImage : Image<Color> {

    public RGBAImage(int width, int height) : base(width, height) { }

    public override Color GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
        throw new System.NotImplementedException();
    }

    public override void SetPixel(int x, int y, Color value) {
        throw new System.NotImplementedException();
    }

    public override byte[] ToByteArray() {
        throw new System.NotImplementedException();
    }

    protected override Color DefaultValue() {
        return Color.black;
    }

}