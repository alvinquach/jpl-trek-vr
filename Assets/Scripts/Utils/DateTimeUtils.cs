using System;

namespace TrekVRApplication {

    public static class DateTimeUtils {

        public static long Now() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

    }

}
