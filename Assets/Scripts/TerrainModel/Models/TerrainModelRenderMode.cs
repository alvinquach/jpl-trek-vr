using System;

namespace TrekVRApplication {

    [Flags]
    public enum TerrainModelRenderMode : int {

        Default = 0,
        Disabled = 1,
        Overlay = 2

    }

}
