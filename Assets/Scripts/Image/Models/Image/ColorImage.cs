using System;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class ColorImage : Image<Color32, byte> {

        public ColorImage(int width, int height) : base(width, height) {

        }

        public override byte[] GetRawPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            byte[] pixel = new byte[DataPerPixel];
            Array.Copy(_rawData, offset, pixel, 0, DataPerPixel);
            return pixel;
        }

    }

}
