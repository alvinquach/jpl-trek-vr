using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class Image<T, DATA> : Image {

        protected abstract int DataPerPixel { get; }

        protected DATA[] _rawData;

        public int Size {
            get => Marshal.SizeOf(_rawData[0]) * _rawData.Length;
        }

        public Image(int width, int height) : base(width, height) {
            _rawData = new DATA[width * height * DataPerPixel];
        }

        /// <summary>
        ///     Sets the value of a pixel. Out of bounds handled; if the coordinates 
        ///     are not within valid range, then this method won't do anything.
        /// </summary>
        public abstract void SetPixel(int x, int y, T value);

        public abstract T GetPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.Repeat);

        public abstract DATA[] GetRawPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None);

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

        public void CopyRawData(DATA[] destinationArray, long destinationIndex = 0) {
            Array.Copy(_rawData, 0, destinationArray, destinationIndex, _rawData.Length);
        }

        protected long GetPixelOffet(int x, int y) {
            return (x + Width * y) * DataPerPixel;
        }

    }

    public abstract class Image {

        public int Width { get; }

        public int Height { get; }

        public Image(int width, int height) {
            Width = width;
            Height = height;
        }

        protected bool IsOutOfBounds(int x, int y) {
            return x < 0 || y < 0 || x > Width - 1 || y > Height - 1;
        }

        protected bool AdjustCoordinates(ref int x, ref int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
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
                    return false;
                }
            }
            return true;
        }

        public static bool operator true(Image o) {
            return o != null;
        }

        public static bool operator false(Image o) {
            return o == null;
        }

        public static bool operator !(Image o) {
            return o ? false : true;
        }

    }

}