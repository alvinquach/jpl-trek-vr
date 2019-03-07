using System;
using UnityEngine;

namespace TrekVRApplication {

    public class BGRAImage {

        private byte[] _rawBytes;

        protected int BytesPerPixel {
            get { return 4; }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Size {
            get { return _rawBytes.Length; }
        }


        public BGRAImage(int width, int height) {
            Width = width;
            Height = height;
            _rawBytes = new byte[width * height * BytesPerPixel];
        }

        public void SetPixel(int x, int y, Color32 value) {
            if (IsOutOfBounds(x, y)) {
                return;
            }
            long offset = GetPixelOffet(x, y);
            _rawBytes[offset] = value.b;
            _rawBytes[offset + 1] = value.g;
            _rawBytes[offset + 2] = value.r;
            _rawBytes[offset + 3] = value.a;
        }

        public Color32 GetPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            return new Color32(
                _rawBytes[offset + 2],
                _rawBytes[offset + 1],
                _rawBytes[offset],
                _rawBytes[offset + 3]
            );
        }

        public byte[] GetRawPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            byte[] pixel = new byte[BytesPerPixel];
            Array.Copy(_rawBytes, offset, pixel, 0, BytesPerPixel);
            return pixel;
        }

        public void CopyRawBytes(byte[] destinationArray, long destinationIndex = 0) {
            Array.Copy(_rawBytes, 0, destinationArray, destinationIndex, _rawBytes.Length);
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

        protected long GetPixelOffet(int x, int y) {
            return (x + Width * y) * BytesPerPixel;
        }

    }

}
