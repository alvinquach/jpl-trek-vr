public class IntensityImage : Image<float> {

    public IntensityImage(int width, int height) : base(width, height) { }

    public override float GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {

        float sum = 0;
        int count = 0;

        for (int j = y; j < y + height; j++) {

            // If the boundary mode is 'None', then ignore the entire row if it is out of bounds.
            if (boundaryMode == ImageBoundaryMode.None && IsOutOfBounds(0, j)) {
                continue;
            }

            for (int i = x; i < x + width; i++) {

                // If the boundary mode is 'None', then ignore the pixel if it is out of bounds.
                if (boundaryMode == ImageBoundaryMode.None && IsOutOfBounds(i, j)) {
                    continue;
                }

                // Call GetPixel() instead of accessing the pixel array directly to handle boundaries.
                sum += GetPixel(i, j, boundaryMode);
                count++;
            }
        }

        return sum / count;
    }

    public override byte[] ToByteArray() {
        // TODO Implement this
        throw new System.NotImplementedException();
    }

    protected override float DefaultValue() {
        return 0.0f;
    }

}