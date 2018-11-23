using UnityEngine;

public static class ImageUtils {

    /// <summary>
    ///     Calculates the starting pixel coordinates (top left corner) of a square
    ///     block of pixels centered around (x, y) with a given width and height.
    /// </summary>
    /// <param name="x">The x-coordinate of the center of the block.</param>
    /// <param name="y">The y-coordinate of the center of the block.</param>
    /// <param name="size">The width and height of the block in pixels.</param>
    /// <returns>The coordinates of the top left pixel of the block.</returns>
    public static Vector2Int CalculateCenteredBlockOffset(int x, int y, int size) {
        return CalculateCenteredBlockOffset(x, y, size, size);
    }

    /// <summary>
    ///     Calculates the starting pixel coordinates (top left corner) of a arbitrary
    ///     block of pixels centered around (x, y) with a given width and height.
    /// </summary>
    /// <param name="x">The x-coordinate of the center of the block.</param>
    /// <param name="y">The y-coordinate of the center of the block.</param>
    /// <param name="width">The width of the block in pixels.</param>
    /// <param name="height">The height of the block in pixels.</param>
    /// <returns>The coordinates of the top left pixel of the block.</returns>
    public static Vector2Int CalculateCenteredBlockOffset(int x, int y, int width, int height) {
        return new Vector2Int(
            1 + (x << 1) + (1 + width) % 2 - width >> 1,
            1 + (y << 1) + (1 + height) % 2 - height >> 1
        );
    }

    /// <summary>
    ///     Given an index of an array that represents a block of pixels, and the width of
    ///     the block, calculates the coordinate of the pixel that corresponds to the index.
    /// </summary>
    /// <param name="index">The index of an array that represent a block of pixels.</param>
    /// <param name="width">The width of the block of pixels.</param>
    /// <returns>The coordinates in the block of pixels that corresponds to the index.</returns>
    public static Vector2Int IndexToCoordinates(int index, int width) {
        int x = index % width;
        int y = (index - x) / width;
        return new Vector2Int(x, y);
    }

    /// <summary>
    ///     Given the width of an image block in pixels and the coordiates of a pixel in the block,
    ///     calcuates the index of the pixel in an array of values that reprsents the block.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel in the block.</param>
    /// <param name="y">The x-coordinate of the pixel in the block.</param>
    /// <param name="width">The width of the block in pixels.</param>
    /// <returns>The index of an array that represent the block of pixels.</returns>
    public static int CoordinatesToIndex(int x, int y, int width) {
        return y * width + x;
    }

}