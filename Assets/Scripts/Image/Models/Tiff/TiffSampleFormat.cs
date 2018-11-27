using System;

namespace TrekVRApplication {

    public class TiffSampleFormat {

        public static readonly TiffSampleFormat SignedShort =
            new TiffSampleFormat(16, (bytes, i) => { return BitConverter.ToInt16(bytes, i); });

        public static readonly TiffSampleFormat SinglePrecisionFloat =
            new TiffSampleFormat(32, BitConverter.ToSingle);

        public delegate float ConvertBitsDelegate(byte[] bytes, int startIndex);

        public int BitsPerSample { get; private set; }

        public ConvertBitsDelegate ConvertBits { get; private set; }

        private TiffSampleFormat(int bytesPerSample, ConvertBitsDelegate convertBits) {
            BitsPerSample = bytesPerSample;
            ConvertBits = convertBits;
        }

    }

}
