using UnityEngine;

public abstract class Image<T> {

    protected readonly T[,] _pixels;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public Image(int width, int height) {
        Width = width;
        Height = height;
        _pixels = new T[width, height];
    }

    /// <summary>
    ///     Sets the value of a pixel. Out of bounds handled; if the coordinates 
    ///     are not within valid range, then this method won't do anything.
    /// </summary>
    public virtual void SetPixel(int x, int y, T value) {
        if (IsOutOfBounds(x, y)) {
            return;
        }
        _pixels[x, y] = value;
    }

    public virtual T GetPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.Repeat) {
        if (IsOutOfBounds(x, y)) {
            if (boundaryMode == ImageBoundaryMode.Wrap) {
                x %= Width;
                y %= Height;
            }
            else if (boundaryMode == ImageBoundaryMode.Repeat) {
                x = MathUtils.Clamp(x, 0, Width - 1);
                y = MathUtils.Clamp(y, 0, Height - 1);
            }
            else {
                return DefaultValue();
            }
        }
        return _pixels[x, y];
    }

    public virtual T GetAverage(int x, int y, int size, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
        return GetAverage(x, y, size, size, boundaryMode);
    }

    public abstract T GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None);

    public virtual T GetCenteredAverage(int x, int y, int size, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
        return GetCenteredAverage(x, y, size, size, boundaryMode);
    }

    public virtual T GetCenteredAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
        Vector2Int offset = ImageUtils.CalculateCenteredBlockOffset(x, y, width, height);
        return GetAverage(offset.x, offset.y, width, height, boundaryMode);
    }

    public abstract byte[] ToByteArray();

    protected abstract T DefaultValue();

    protected bool IsOutOfBounds(int x, int y) {
        return x < 0 || y < 0 || x > Width - 1 || y > Height - 1;
    }

    public static bool operator true(Image<T> o) {
        return o != null;
    }

    public static bool operator false(Image<T> o) {
        return o == null;
    }

    public static bool operator !(Image<T> o) {
        return o ? false : true;
    }

}